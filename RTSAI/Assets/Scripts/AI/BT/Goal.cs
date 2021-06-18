public abstract class Goal : Node
{
    GoalPriority m_GoalPriority;

    public EGoal GoalState
    {
        get { return m_GoalPriority.Goal; }
        set { m_GoalPriority.Goal = value; }
    }

    public EPriority PriorityLevel
    {
        get { return m_GoalPriority.Priority; }
        set { m_GoalPriority.Priority = value; }
    }
}
