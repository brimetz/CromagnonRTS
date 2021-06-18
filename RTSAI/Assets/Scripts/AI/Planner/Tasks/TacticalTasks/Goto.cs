using UnityEngine;

public class Goto : TacticalTask
{
    public Vector3 Location;
    public EPosture SquadPosture = EPosture.Passive;

    public override void Init()
    {
        WorldPrecondition = new int[] { 0, -1, -1, -1, -1, -1 };
        WorldPostCondition = new int[] { 1, -1, -1, -1, -1, -1 };
    }

    public override void Execute(Squad squad)
    {
        squad.SetPosture(SquadPosture);
        squad.SetPositionTarget(Location);
    }

    public override void Update(Squad squad)
    {
        if (squad.IsArrived())
            m_IsFinish = true;
    }
}
