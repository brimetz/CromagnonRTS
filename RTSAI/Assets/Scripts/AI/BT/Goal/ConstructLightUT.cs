using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructLightUT : Goal
{
    public ConstructLightUT()
    {
        PriorityLevel = EPriority.VeryLow;
        GoalState = EGoal.ConstructLightUT;
    }

    public override NodeStatus OnBehave(BehaviourState state)
    {
        Context context = state as Context;

        bool hasHeavyFactory = false;
        for (int i = 0; i < context.AI.FactoryList.Count; i++)
        {
            if (context.AI.FactoryList[i].name.Contains("Heavy"))
            {
                hasHeavyFactory = true;
                break;
            }
        }

        if ((context.AI.TotalBuildPoints >= 3 && context.AI.CapturedTargets <= 3) || (context.AI.UnitList.Count == 0 && !hasHeavyFactory) 
            || (context.AI.TotalBuildPoints > 4 && hasHeavyFactory))
            return NodeStatus.Success;
        else
            return NodeStatus.Failure;
    }

    public override void OnReset()
    {
    }
}
