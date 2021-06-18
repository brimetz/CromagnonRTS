public class Idle : TacticalTask
{
    public float WaitingTime = 0.0f;

    public override void Init()
    {
        WorldPrecondition = new int[] { 1, -1, -1, 0, -1, -1 };
        WorldPostCondition = new int[] { 1, -1, -1, 1, -1, -1 };
    }

    public override void Execute(Squad squad)
    {
    }

    public override void Update(Squad squad)
    {
    }
}
