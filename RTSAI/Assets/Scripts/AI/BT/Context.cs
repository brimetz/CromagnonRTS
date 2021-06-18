public class Context : BehaviourState
{
    public AIController AI;
    public GoalPriority EndGoal = new GoalPriority();

    public Context(AIController AI)
    {
        this.AI = AI;
        EndGoal.Goal = EGoal.None;
        EndGoal.Priority = EPriority.None;
    }
}
