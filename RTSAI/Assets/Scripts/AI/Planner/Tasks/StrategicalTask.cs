public abstract class StrategicalTask : Task
{
    public abstract void Init();

    public abstract bool CheckPreCondition(AIController AI, EGoal goal, int[] currentState, int[] finalState);

    public abstract void PostCondition(ref int[] currentState, int[] finalState);

    public abstract void Execute(AIController AI);

    public abstract void Update(AIController AI);
}
