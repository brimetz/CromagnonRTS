public class UpdateSquad : StrategicalTask
{
    private Squad m_SquadToUpdate = null;
    public ETacticalGoal TacticalGoal = ETacticalGoal.Flee;

    public override void Init()
    {
    }

    public override bool CheckPreCondition(AIController AI, EGoal goal, int[] currentState, int[] finalState)
    {
        // if we need to update a squad to achieve the plan, and there is more than 0 squad in Idle
        if (currentState[5] < finalState[5] && currentState[4] > 0)
            return true;

        return false;
    }

    public override void PostCondition(ref int[] currentState, int[] finalState)
    {
        currentState[5]++;
    }

    public override void Execute(AIController AI)
    {
        m_SquadToUpdate = AI.GetRandomIdleSquad(TacticalGoal);
        AI.UpdateSquad(m_SquadToUpdate, TacticalGoal);
    }

    public override void Update(AIController AI)
    {
        IsFinish = true;
    }
}
