public class Reparation : Goal
{
    public Reparation()
    {
        PriorityLevel = EPriority.High;
        GoalState = EGoal.Repair;
    }

    public override NodeStatus OnBehave(BehaviourState state)
    {
        Context context = state as Context;

        for (int i = 0; i < context.AI.FactoryList.Count; i++)
        {
            int ThresholdHP = context.AI.FactoryList[i].FactoryData.MaxHP - (context.AI.FactoryList[i].FactoryData.MaxHP / 3);
            if (context.AI.FactoryList[i].GetHP() < ThresholdHP)
                return NodeStatus.Success;
        }

        int NbUnitNotFullHP = 0;
        for (int i = 0; i < context.AI.UnitList.Count; i++)
        {
            if (context.AI.UnitList[i].GetHP() < context.AI.UnitList[i].GetUnitData.MaxHP - 20)
                NbUnitNotFullHP++;
        }

        if (NbUnitNotFullHP > 6)
            return NodeStatus.Success;

        return NodeStatus.Failure;
    }

    public override void OnReset()
    {
    }
}
