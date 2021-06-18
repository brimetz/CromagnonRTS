using System.Collections.Generic;
using UnityEngine;

// Goap Planner with plan built in one pass
public class StrategicalPlanner : Planner
{
    private List<StrategicalTask> m_TaskLibrairie = new List<StrategicalTask>();

    public void Init()
    {
        // Load the librairy of task possible in this planner
        m_TaskLibrairie.Add(new CreateLightFactory());
        m_TaskLibrairie.Add(new CreateHeavyFactory());
        m_TaskLibrairie.Add(new CreateLightUT());
        m_TaskLibrairie.Add(new CreateHeavyUT());
        m_TaskLibrairie.Add(new OrganizeArmy());
        m_TaskLibrairie.Add(new UpdateSquad());
    }

   public StrategicalPlan GetPlan(EGoal goal, AIController AI)
    {
        // Compute the initial state and the goal state
        int[] initialState = ComputeInitialState(AI);
        int[] finalState = ComputeFinalState(initialState, goal);

        // Create and Init the plan
        StrategicalPlan newPlan = new StrategicalPlan();
        newPlan.SetAI(AI);

        // Try to build the plan
        BuildChain(AI, initialState, finalState, goal, ref newPlan);
        
        return newPlan;
    }

    private int[] ComputeInitialState(AIController AI)
    {
        // State = Ressources, FactoryLight, FactoryHeavy, IdleUnit, IdleSquad, UpdateSquad
        return new int[]
        {
            AI.TotalBuildPoints,
            AI.GetNumberLightFactory(),
            AI.GetNumberHeavyFactory(),
            AI.GetIdleUT(),
            AI.GetIdleSquad(),
            0
        };
    }

    private int[] ComputeFinalState(int[] initialState, EGoal goal)
    {
        // State = Ressources, FactoryLight, FactoryHeavy, IdleUnit, IdleSquad, UpdateSquad
        return goal switch
		{
			EGoal.Conquer => new int[] { -1, -1, -1, -1, 1, 1 },
			EGoal.ConstructHeavyUT => new int[] { 5, -1, 1, initialState[3] + 1, -1, 0 },
			EGoal.ConstructLightUT => new int[] { 3, 1, -1, initialState[3] + 4, -1, 0 },
			EGoal.Defend => new int[] { -1, -1, -1, -1, 1, 1 },
			EGoal.Destroy => new int[] { -1, -1, -1, -1, 1, 1 },
			EGoal.Explore => new int[] { -1, -1, -1, -1, 1, 1 },
			EGoal.Secure => new int[] { -1, -1, -1, -1, 1, 1 },
			EGoal.Repair => new int[] { -1, -1, -1, -1, 1, 1 },
			_ => initialState,
		};
	}

    private void BuildChain(AIController AI, int[] initialState, int[] finalState, EGoal goal, ref StrategicalPlan newPlan)
    {
        // Create a initial Plan
        StrategicalPlan basePlan = new StrategicalPlan();
        basePlan.SetAI(AI);

        KeyValuePair<int[], StrategicalPlan> initialSearchState = new KeyValuePair<int[], StrategicalPlan>(initialState, basePlan);

        // List of the State to search with the Goap Algo
		List<KeyValuePair<int[], StrategicalPlan>> openSearchStates = new List<KeyValuePair<int[], StrategicalPlan>>
		{
			initialSearchState
		};

        // Goap Algo 
		while (openSearchStates.Count != 0)
        {
            for (int i = 0; i < m_TaskLibrairie.Count; i++)
            {
                // Check if One task can be usefull to achieve the plan
                int[] currentState = openSearchStates[0].Key;
                if (m_TaskLibrairie[i].CheckPreCondition(AI, goal, currentState, finalState))
                {
                    StrategicalTask newTask = InitTask(i, AI, goal);

                    newTask.PostCondition(ref currentState, finalState);

                    openSearchStates[0].Value.AddTask(newTask);

                    // If Plan Post Condition = Final State, the plan is finish
                    if (CheckPlanIsFinish(currentState, finalState))
                    {
                        newPlan = openSearchStates[0].Value;
                        return;
                    }

                    // Need a condition here to done the right Goap algo, but I don't need in my implementation because in One pass, the plan is build
                    openSearchStates.Add(new KeyValuePair<int[], StrategicalPlan>(currentState, openSearchStates[0].Value));
                }
            }
            openSearchStates.RemoveAt(0);
        }
    }

    private StrategicalTask InitTask(int taskIndex, AIController AI, EGoal goal)
    {
        switch (taskIndex)
        {
            case 0:
				CreateLightFactory taskCreateLFact = new CreateLightFactory
				{
					Location = AI.GetLocationNewBuilding()
				};
				Debug.Log("StrategicalPlanner::InitTask - CreateLightFactory");
                return taskCreateLFact;
            case 1:
				CreateHeavyFactory taskCreateHFact = new CreateHeavyFactory
				{
					Location = AI.GetLocationNewBuilding()
				};
                Debug.Log("StrategicalPlanner::InitTask - CreateHeavyFactory");
                return taskCreateHFact;
            case 2:
                CreateLightUT taskCreateLUT = new CreateLightUT();
                Debug.Log("StrategicalPlanner::InitTask - CreateLightUT");
                return taskCreateLUT;
            case 3:
                CreateHeavyUT taskCreateHUT = new CreateHeavyUT();
                Debug.Log("StrategicalPlanner::InitTask - CreateHeavyUT");
                return taskCreateHUT;
            case 4:
                OrganizeArmy taskOrganizeArmy = new OrganizeArmy();
                taskOrganizeArmy.NeededUnit = (m_TaskLibrairie[4] as OrganizeArmy).NeededUnit;
                taskOrganizeArmy.m_SquadSearchingRecrue = (m_TaskLibrairie[4] as OrganizeArmy).m_SquadSearchingRecrue;
                Debug.Log("StrategicalPlanner::InitTask - OrganizeArmy");
                return taskOrganizeArmy;
            case 5:
				UpdateSquad taskUpdateSquad = new UpdateSquad
				{
					TacticalGoal = TraductGoal(goal)
				};
                Debug.Log("StrategicalPlanner::InitTask - UpdateSquad");
                return taskUpdateSquad;
            default:
                return null;
        }
    }

    private ETacticalGoal TraductGoal(EGoal stratGoal)
    {
		return stratGoal switch
		{
			EGoal.Conquer => ETacticalGoal.Conquer,
			EGoal.Defend => ETacticalGoal.Defend,
			EGoal.Destroy => ETacticalGoal.Destroy,
			EGoal.Explore => ETacticalGoal.Explore,
			EGoal.Secure => ETacticalGoal.Secure,
			EGoal.Repair => ETacticalGoal.Repair,
			_ => ETacticalGoal.Explore,
		};
	}

    private bool CheckPlanIsFinish(int[] currentState, int[] finalState)
    {
        return ((currentState[1] == finalState[1] || finalState[1] == -1) &&
            (currentState[2] == finalState[2] || finalState[2] == -1) &&
            (currentState[3] >= finalState[3] || finalState[3] == -1) &&
            (currentState[5] == finalState[5] || finalState[5] == -1));
    }
}
