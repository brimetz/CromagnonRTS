public class Conquer : Goal
{
    public Conquer()
    {
        PriorityLevel = EPriority.Low;
        GoalState = EGoal.Conquer;
    }

    public override NodeStatus OnBehave(BehaviourState state)
    {
        Context context = state as Context;

        int nbTargetCaptured = 0;
        int nbOwnTarget = 0;

        for (int i = 0; i < context.AI.TargetBuildingsSeen.Count; i++)
            if (context.AI.TargetBuildingsSeen[i].GetTeam() == context.AI.GetTeam())
                nbOwnTarget++;

        for (int i = 0; i < context.AI.TargetBuildingsSeen.Count; i++)
            if (context.AI.TargetBuildingsSeen[i].GetTeam() == context.AI.GetTeam().GetEnemyTeam())
                nbTargetCaptured++;

        if (context.AI.TargetBuildingsSeen.Count > 2 && context.AI.TargetBuildingsSeen.Count - nbOwnTarget > 0 &&
            (context.AI.CapturedTargets < nbTargetCaptured+2 || context.AI.CapturedTargets == 0)
            && context.AI.GetSquadOnTask(ETacticalGoal.Conquer) < 3)
            return NodeStatus.Success;
        else
            return NodeStatus.Failure;
    }

    public override void OnReset()
    {
    }
}
