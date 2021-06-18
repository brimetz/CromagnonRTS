public class Repair : TacticalTask
{
    public BaseEntity Target = null;

    public override void Init()
    {
        WorldPrecondition = new int[] { 1, -1, -1, -1, -1 , 0};
        WorldPostCondition = new int[] { 1, -1, -1, -1, -1, 1};
    }

    public override void Execute(Squad squad)
    {
        squad.SetRepairTarget(Target);
    }

    public override void Update(Squad squad)
    {
        if (!squad.CanRepair())
            m_IsFinish = true;

        if (!Target.NeedsRepairing() || !squad.CanRepair())
            m_IsFinish = true;
    }
}