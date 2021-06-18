public class OrganizeArmy : StrategicalTask
{
    public Squad m_SquadSearchingRecrue = null;
    public int NeededUnit = 0;

    public override void Init()
    {
    }

    public override bool CheckPreCondition(AIController AI, EGoal goal, int[] currentState, int[] finalState)
    {
        m_SquadSearchingRecrue = null;
        NeededUnit = 0;
        if (finalState[4] > currentState[4])
        {
            // If To many Squad is in Research of Unit, we just disband one squad
            if (AI.NbSquadSearchingRecrue() > 1)
                AI.DisbandSquad(AI.SearchSquadWaitingRecrue());

            NeededUnit = AI.SearchSquadWaitingRecrue(currentState[3], ref m_SquadSearchingRecrue);

            // If a squad search some unit, We send Unit to fill the squad
            if (m_SquadSearchingRecrue != null)
                return true;
            // If nbUnit in Idle > nbUnit in Squad, I form a new Squad
            else if (currentState[3] >= 3)
            {
                NeededUnit = 4;
                return true;
            }
        }
        return false;
    }

    public override void PostCondition(ref int[] currentState, int[] finalState)
    {
        // Remove Nb unit used to create or fill a squad, and add one squad in idle
        currentState[3] -= NeededUnit;
        currentState[4]++;
    }

    public override void Execute(AIController AI)
    {
        if (m_SquadSearchingRecrue != null)
        {
            for (int i = 0; i < NeededUnit; i++)
                m_SquadSearchingRecrue.AddUT(AI.GetIdleUTRandom());
        }
        else
        {
            Squad NewSquad = new Squad();
            NewSquad.SetTeam(AI.GetTeam());
            AI.AddSquad(NewSquad);

            for (int i = 0; i < NeededUnit; i++)
                NewSquad.AddUT(AI.GetIdleUTRandom());
        }
    }

    public override void Update(AIController AI)
    {
        IsFinish = true;
    }
}
