using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defend : Goal
{
    public Defend()
    {
        PriorityLevel = EPriority.VeryHigh;
        GoalState = EGoal.Defend;
    }

    public override NodeStatus OnBehave(BehaviourState state)
    {
        Context context = state as Context;

        for (int i = 0; i < context.AI.FactoryList.Count; i++)
            if (context.AI.FactoryList[i].IsUnderAttack)
                return NodeStatus.Success;
        return NodeStatus.Failure;
    }

    public override void OnReset()
    {
    }
}
