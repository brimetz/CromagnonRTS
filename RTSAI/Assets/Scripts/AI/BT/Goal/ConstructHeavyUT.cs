public class ConstructHeavyUT : Goal
{
    public ConstructHeavyUT()
    {
        PriorityLevel = EPriority.Medium;
        GoalState = EGoal.ConstructHeavyUT;
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

        if ((context.AI.TotalBuildPoints >= 23 && context.AI.CapturedTargets > 3 && !hasHeavyFactory && context.AI.UnitList.Count > 10) || (hasHeavyFactory && context.AI.TotalBuildPoints >= 6))
            return NodeStatus.Success;
        return NodeStatus.Failure;
    }

    public override void OnReset()
    {
    }
}
