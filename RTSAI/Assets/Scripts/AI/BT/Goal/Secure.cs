public class Secure : Goal
{
    public Secure()
    {
        PriorityLevel = EPriority.Medium;
        GoalState = EGoal.Secure;
    }

    public override NodeStatus OnBehave(BehaviourState state)
    {
        Context context = state as Context;

        int nbTargetCaptured = 0;
        int nbTargetSecured = 0;

        for (int i = 0; i < context.AI.TargetBuildingsSeen.Count; i++)
        {
            if (context.AI.TargetBuildingsSeen[i].GetTeam() == context.AI.GetTeam())
            {
                nbTargetCaptured++;
                if (context.AI.TargetBuildingsSeen[i].secure)
                    nbTargetSecured++;
            }
        }

        if (nbTargetCaptured - nbTargetSecured > 3 && context.AI.UnitList.Count > (4 * nbTargetSecured) + 7)
            return NodeStatus.Success;
        else
            return NodeStatus.Failure;
    }

    public override void OnReset()
    {
    }
}
