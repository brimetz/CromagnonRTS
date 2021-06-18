public enum EUnit
{
    HeavyTankA,
    HeavyTankB,
    Quad,
    Trike,
    Infantry
}

public class UnitContainer
{
    public EUnit UnitType;
    public int DPS;

    public UnitContainer(EUnit unitType, int dps)
    {
        UnitType = unitType;
        DPS = dps;
    }
}
