public class CarryOn : TacticalTask
{
    public int IdNextTask = 0;
    public TacticalPlan Plan = null;

    public override void Init()
    {
        // This task is special, because it doesn't need any condition, we need to put it in plan self
        WorldPrecondition = new int[] { -1, -1, -1, -1, -1, -1 };
        WorldPostCondition = new int[] { -1, -1, -1, -1, -1, -1 };
    }

    public override void Execute(Squad squad)
    {
        Plan.CurrentTask = IdNextTask;
        Plan.SetNeedRepeat(true);
    }

    public override void Update(Squad squad)
    {
    }
}
