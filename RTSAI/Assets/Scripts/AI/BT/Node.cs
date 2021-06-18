using System.Collections.Generic;
using UnityEngine;

public enum NodeStatus
{
    Failure,
    Success,
    Running
}

public abstract class Node
{
    public static List<string> s_DebugTypeBlacklist = new List<string>()
    {
        "Selector",
        "Sequence",
        "Repeater",
        "Inverter",
        "Succeeder"
    };

    public bool CanStart = true;
    public int Ticks = 0;
    public GoalPriority GoalPriority = new GoalPriority();
    public Goal GoalNode = null;
    protected bool m_Debug = false;

    public virtual NodeStatus Behave(BehaviourState state)
    {
        NodeStatus nodeStatus = OnBehave(state);

        if (m_Debug && !s_DebugTypeBlacklist.Contains(GetType().Name))
        {
            string result = "Unknown";
            switch (nodeStatus)
            {
                case NodeStatus.Success:
                    result = "Success";
                    break;
                case NodeStatus.Failure:
                    result = "Failure";
                    break;

                case NodeStatus.Running:
                    result = "Running";
                    break;
            }
            Debug.Log("Node::Behave - " + GetType().Name + ": " + result);
        }
        
        switch (nodeStatus)
        {
            case NodeStatus.Success:
                if (this is Goal)
                {
                    if (GoalNode == null)
                        GoalNode = this as Goal;
                    else if ((this as Goal).PriorityLevel > GoalNode.PriorityLevel)
                        GoalNode = this as Goal;
                }
                break;
            case NodeStatus.Failure: break;
            case NodeStatus.Running: break;
        }

        Ticks++;
        CanStart = false;

        FillGoal(state);
        if (nodeStatus != NodeStatus.Running)
            Reset();

        return nodeStatus;
    }

    public void FillGoal(BehaviourState state)
    {
        if (GoalNode != null)
        {
            Context context = state as Context;
            if (context.EndGoal.Priority < GoalNode.PriorityLevel)
            {

                context.EndGoal.Goal = GoalNode.GoalState;
                context.EndGoal.Priority = GoalNode.PriorityLevel;
            }
        }
    }

    public void Reset()
    {
        GoalNode = null;
        CanStart = true;
        Ticks = 0;
        OnReset();
    }

    public abstract NodeStatus OnBehave(BehaviourState state);
    public abstract void OnReset();
}
