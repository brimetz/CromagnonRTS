using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Goap SAS+PUS Planner with state in binary value
public class TacticalPlanner : Planner
{
    private List<TacticalTask> m_TaskLibrairie = new List<TacticalTask>();

    public void Init()
    {
        // Load the librairy of task possible in this planner
        m_TaskLibrairie.Add(new Capture());
        m_TaskLibrairie.Add(new Attack());
        m_TaskLibrairie.Add(new Idle());
        m_TaskLibrairie.Add(new SearchRecrue());
        m_TaskLibrairie.Add(new Repair());
        m_TaskLibrairie.Add(new Goto());
        m_TaskLibrairie.Add(new CarryOn());

        // Init each task to compute the Précondition and postCondition
        foreach (TacticalTask Task in m_TaskLibrairie)
            Task.Init();
    }

    public TacticalPlan GetPlan(ETacticalGoal goal, Squad squad, Target target)
    {
        // Compute the initial state and the goal state
        int[] finalState = ComputeFinalState(goal);
        int[] startState = ComputeStartState(goal);

        // Create and Init the plan
        TacticalPlan newPlan = new TacticalPlan();
        newPlan.SetSquad(squad);

        if (goal == ETacticalGoal.Secure) 
        {
            // Add a Carry on at the end of the plan
            finalState = AddTask(6, target, goal, ref newPlan, ref finalState);
        }

        // Try to Build the plan, we recurcive function
        BuildChain(startState, finalState, target, goal, ref newPlan);

        // Set Squad variable
        squad.CurrentPlan = newPlan;
        squad.currentFormation = GetFormation(goal);

        return newPlan;
    }

    private int[] ComputeStartState(ETacticalGoal goal)
    {
        // State = IsAtLocation, TargetIsCaptured, TargetIsDead, TimerIsFinish, SquadIsFull, TargetIsRepaired
		return goal switch
		{
			ETacticalGoal.Conquer => new int[] { 0, 0, 0, -1, -1, -1 },
			ETacticalGoal.Defend => new int[] { 0, -1, -1, -1, -1, -1 },
			ETacticalGoal.Destroy => new int[] { 0, -1, 0, -1, -1, -1 },
			ETacticalGoal.Explore => new int[] { 0, -1, -1, -1, -1, -1 },
			ETacticalGoal.Flee => new int[] { 0, -1, -1, -1, 0, -1 },
			ETacticalGoal.Repair => new int[] { 0, -1, -1, -1, -1, 0 },
			ETacticalGoal.Secure => new int[] { 0, -1, -1, -1, -1, -1 },
			_ => new int[0],
		};
	}

    private int[] ComputeFinalState(ETacticalGoal goal)
    {
        // State = IsAtLocation, TargetIsCaptured, TargetIsDead, TimerIsFinish, SquadIsFull, TargetIsRepaired
        return goal switch
		{
			ETacticalGoal.Conquer => new int[] { 1, 1, 0, -1, -1, -1 },
			ETacticalGoal.Defend => new int[] { 1, -1, -1, -1, -1, -1 },
			ETacticalGoal.Destroy => new int[] { 1, -1, 1, -1, -1, -1 },
			ETacticalGoal.Explore => new int[] { 1, -1, -1, -1, -1, -1 },
			ETacticalGoal.Flee => new int[] { 1, -1, -1, -1, 1, -1 },
			ETacticalGoal.Repair => new int[] { 1, -1, -1, -1, -1, 1 },
			ETacticalGoal.Secure => new int[] { 1, -1, -1, -1, -1, -1 },
			_ => new int[0],
		};
	}

    private void BuildChain(int[] startState, int[] finalState, Target targetStruct, ETacticalGoal goal, ref TacticalPlan builtPlan)
    {
        // If StartState == FinalState, Plan is finish
        if (startState.SequenceEqual(finalState))
            return;

        // Find Action usefull to achieve the plan
        int IdTask = FindActionPostCondition(finalState);
        if (IdTask < 0)
        {
            return;
        }
        else
        {
            // Get the next postCondition from the task added to the plan
            int[] NewPostCondition = AddTask(IdTask, targetStruct, goal, ref builtPlan, ref finalState);
            
            // Recall the function to reiterate on the plan
            BuildChain(startState, NewPostCondition, targetStruct, goal, ref builtPlan);
        }
    }

    private int FindActionPostCondition(int[] postCondition)
    {
        // Goap SAS + PUS Planner Search algo
        int index;
        bool goodAction = true;

        // We can't find CarryOn Here, Because CarryOn is a special Task who can feet for all state, so nbTask - 1
        for (int i = 0; i < m_TaskLibrairie.Count - 1; i++)
        {
            index = 0;
            goodAction = true;
            while (index < 6)
            {
                if (m_TaskLibrairie[i].WorldPostCondition[index] == postCondition[index] || m_TaskLibrairie[i].WorldPostCondition[index] == -1)
                {
                }
                else
                {
                    goodAction = false;
                    break;
                }
                index++;
            }
            if (goodAction)
                return i;
        }
        return -1;
    }

    private void UpdateFinalState(ref int[] finalState, TacticalTask task)
    {
        // Update Final State with the last task added
        for (int i = 0; i < 6; i++)
        {
            if(task.WorldPrecondition[i] != -1 && task.WorldPrecondition[i] != finalState[i])
            {
                finalState[i] = task.WorldPrecondition[i];
                return;
            }
        }
    }

    private int[] AddTask(int idTask, Target target, ETacticalGoal goal, ref TacticalPlan builtPlan, ref int[] finalState)
    {
        switch (idTask)
        {
            case 0:
                Capture taskCapture = new Capture();
                taskCapture.Init();
                taskCapture.Target = target.Building;
                builtPlan.AddForwardTask(taskCapture);
                Debug.Log("TacticalPlanner::AddTask - Capture");
                UpdateFinalState(ref finalState, taskCapture);
                return finalState;
            case 1:
                Attack taskAttack = new Attack();
                taskAttack.Init();
                taskAttack.Target = target.Entity;
                builtPlan.AddForwardTask(taskAttack);
                Debug.Log("TacticalPlanner::AddTask - Attack");
                UpdateFinalState(ref finalState, taskAttack);
                return finalState;
            case 2:
                Idle taskIdle = new Idle();
                taskIdle.Init();
                taskIdle.WaitingTime = target.Time;
                builtPlan.AddForwardTask(taskIdle);
                Debug.Log("TacticalPlanner::AddTask - Idle");
                UpdateFinalState(ref finalState, taskIdle);
                return finalState;
            case 3:
                SearchRecrue taskSearchRecrue = new SearchRecrue();
                taskSearchRecrue.Init();
                builtPlan.AddForwardTask(taskSearchRecrue);
                Debug.Log("TacticalPlanner::AddTask - SearchRecrue");
                UpdateFinalState(ref finalState, taskSearchRecrue);
                return finalState;
            case 4:
                Repair taskRepair = new Repair();
                taskRepair.Init();
                taskRepair.Target = target.Entity;
                Debug.Log("TacticalPlanner::AddTask - Repair");
                builtPlan.AddForwardTask(taskRepair);
                UpdateFinalState(ref finalState, taskRepair);
                return finalState;
            case 5:
                return ComputeGoto(target, goal, ref builtPlan, ref finalState);
            case 6:
                CarryOn taskCarryOn = new CarryOn();
                taskCarryOn.Init();
                taskCarryOn.IdNextTask = target.TaskToRepeat;
                taskCarryOn.Plan = builtPlan;
                builtPlan.AddForwardTask(taskCarryOn);
                Debug.Log("TacticalPlanner::AddTask - CarryOn");
                UpdateFinalState(ref finalState, taskCarryOn);
                return finalState;
            default:
                return new int[0];
        }
    }

    private int[] ComputeGoto(Target target, ETacticalGoal goal, ref TacticalPlan builtPlan, ref int[] finalState)
    {
        EPosture posture = GetPostureMode(goal);

        // If Waypoints.count == 0, used the location, else use a goto for each waypoint
        if (target.Waypoints.Count == 0)
        {
            Goto taskGoto = new Goto();
            taskGoto.Init();
            taskGoto.Location = target.Location;
            taskGoto.SquadPosture = posture;
            Debug.Log("TacticalPlanner::ComputeGoto - Goto");
            builtPlan.AddForwardTask(taskGoto);
            UpdateFinalState(ref finalState, taskGoto);
            return finalState;
        }
        else
        {
            Goto taskGoto = null;
            for (int i = 0; i < target.Waypoints.Count; i++)
            {
                taskGoto = new Goto();
                taskGoto.Init();
                taskGoto.Location = target.Waypoints[i];
                taskGoto.SquadPosture = posture;
                Debug.Log("TacticalPlanner::ComputeGoto - Goto");
                builtPlan.AddForwardTask(taskGoto);
            }
            UpdateFinalState(ref finalState, taskGoto);
            return finalState;
        }
    }

    private EFormation GetFormation(ETacticalGoal goal)
    {
		return goal switch
		{
			ETacticalGoal.Conquer => EFormation.Triangle,
			ETacticalGoal.Defend => EFormation.Square,
			ETacticalGoal.Destroy => EFormation.Triangle,
			ETacticalGoal.Explore => EFormation.Line,
			ETacticalGoal.Flee => EFormation.Column,
			ETacticalGoal.Repair => EFormation.Square,
			ETacticalGoal.Secure => EFormation.Square,
			_ => EFormation.Line,
		};
	}

    private EPosture GetPostureMode(ETacticalGoal goal)
    {
		return goal switch
		{
			ETacticalGoal.Conquer => EPosture.Aggressive,
			ETacticalGoal.Defend => EPosture.Aggressive,
			ETacticalGoal.Destroy => EPosture.Aggressive,
			ETacticalGoal.Explore => EPosture.Passive,
			ETacticalGoal.Flee => EPosture.Passive,
			ETacticalGoal.Repair => EPosture.Repair,
			ETacticalGoal.Secure => EPosture.Aggressive,
			_ => EPosture.Passive,
		};
	}
}
