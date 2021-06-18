public enum EGoal
{
    None,
    Conquer,
    Defend,
    Destroy,
    Explore,
    ConstructHeavyUT,
    ConstructLightUT,
    Secure,
    Repair
};

public enum EPriority : int
{
    None = 0,
    VeryLow,
    Low,
    Medium,
    High,
    VeryHigh
};

public struct GoalPriority
{
    public EGoal Goal;
    public EPriority Priority;
}
