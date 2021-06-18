public class SearchRecrue : TacticalTask
{
    public override void Init()
    {
        WorldPrecondition = new int[] { 1, -1, -1, -1, 0, -1 };
        WorldPostCondition = new int[] { 1, -1, -1, -1, 1, -1 };
    }

    public override void Execute(Squad squad)
    {
        squad.SetWaitingForRecrue();
    }

    public override void Update(Squad squad)
    {
        if (squad.IsFull())
            m_IsFinish = true;
    }
}
