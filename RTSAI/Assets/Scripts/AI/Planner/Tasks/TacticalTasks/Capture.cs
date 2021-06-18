public class Capture : TacticalTask
{
    public TargetBuilding Target = null;

    public override void Init()
    {
        WorldPrecondition = new int[] { 1, 0, 0, -1, -1, -1 };
        WorldPostCondition = new int[] { 1, 1, 0, -1, -1, -1 };
    }

    public override void Execute(Squad squad)
    {
        squad.SetCaptureTarget(Target);
    }

    public override void Update(Squad squad)
    {
        if (Target.GetTeam() == squad.GetTeam())
            m_IsFinish = true;
    }
}
