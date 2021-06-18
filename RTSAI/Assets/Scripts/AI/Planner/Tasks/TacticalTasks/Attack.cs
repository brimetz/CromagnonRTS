public class Attack : TacticalTask
{
    public BaseEntity Target = null;

    public override void Init()
    {
        WorldPrecondition = new int[] { 1, -1, 0, -1, -1, -1 };
        WorldPostCondition = new int[] { 1, -1, 1, -1, -1, -1 };
    }

    public override void Execute(Squad squad)
    {
        if (Target == null)
            return; 

        squad.SetAttackTarget(Target);
    }

    public override void Update(Squad squad)
    {
        if (!Target || !Target.IsAlive)
            m_IsFinish = true;
    }
}
