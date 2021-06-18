public class Explore : Goal
{
    public Explore()
    {
        PriorityLevel = EPriority.Low;
        GoalState = EGoal.Explore;
    }

    public override NodeStatus OnBehave(BehaviourState state)
    {
        Context context = state as Context;

        if (context.AI.UnitList.Count > 0 && context.AI.GetSquadOnTask(ETacticalGoal.Explore) < 3)
            return NodeStatus.Success;
        else
            return NodeStatus.Failure;
    }

    public override void OnReset()
    {
    }
}
