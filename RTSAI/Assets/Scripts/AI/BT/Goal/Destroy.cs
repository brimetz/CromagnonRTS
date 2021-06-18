public class Destroy : Goal
{
    private float m_RatioUT = 1.2f;
    private float m_RatioFactory = 1.5f;

    public Destroy()
    {
        PriorityLevel = EPriority.VeryHigh;
        GoalState = EGoal.Destroy;
    }

    public override NodeStatus OnBehave(BehaviourState state)
    {
        Context context = state as Context;

        int nbTargetCaptured = 0;
        for (int i = 0; i < context.AI.TargetBuildingsSeen.Count; i++)
            if (context.AI.TargetBuildingsSeen[i].GetTeam() == context.AI.GetTeam().GetEnemyTeam())
                nbTargetCaptured++;

        if (context.AI.UnitList.Count > 10 && 
            context.AI.FactoriesEnemySeen.Count > 0 && context.AI.FactoryList.Count >= 2 &&
            (context.AI.UnitList.Count > context.AI.UnitsEnemySeen.Count * m_RatioUT ||
            context.AI.CapturedTargets > nbTargetCaptured * m_RatioFactory))
            return NodeStatus.Success;
        else
            return NodeStatus.Failure;
    }

    public override void OnReset()
    {
    }
}
