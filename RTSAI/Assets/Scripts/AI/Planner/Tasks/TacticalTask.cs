public abstract class TacticalTask : Task
{
    public int[] WorldPrecondition;
    public int[] WorldPostCondition;

    public abstract void Init();

    public abstract void Execute(Squad squad);

    public abstract void Update(Squad squad);
}
