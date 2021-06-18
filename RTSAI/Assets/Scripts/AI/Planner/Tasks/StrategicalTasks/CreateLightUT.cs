using UnityEngine;

public class CreateLightUT : StrategicalTask
{
    public int NbUnit = 4;

    private int m_UsedRessources = 0;
    private Factory m_FactoryUsed = null;
    private int m_IndexUT = Random.Range(0, 3);

    public override void Init()
    {
    }

    public override bool CheckPreCondition(AIController AI, EGoal goal, int[] currentState, int[] finalState)
    {
        if ((AI.UnitList.Count >= 15 || AI.CapturedTargets > 3) && currentState[2] == 0)
            return false;

        if (currentState[0] >= 1 && currentState[1] > 0 && 
            ((currentState[3] < 2 || currentState[4] == 0) && goal != EGoal.ConstructHeavyUT))
        {
            if (currentState[0] < 6)
                m_IndexUT = 0;
            else if (currentState[0] < 16)
                while (m_IndexUT == 2)
                    m_IndexUT = Random.Range(0, 2);

            if (goal == EGoal.Repair)
                m_IndexUT = 0;

            for (int i = 0; i < NbUnit; i++)
                m_UsedRessources += AI.GetCostLightUT(m_IndexUT);

            return true;
        }
        return false;
    }

    public override void PostCondition(ref int[] currentState, int[] finalState)
    {
        currentState[0] -= m_UsedRessources;
        currentState[3] += NbUnit;
    }

    public override void Execute(AIController AI)
    {
        for (int i = 0; i < NbUnit; i++)
            AI.BuildLightUT(m_IndexUT, ref m_FactoryUsed);
    }

    public override void Update(AIController AI)
    {
        if (m_FactoryUsed != null)
        {
            if (m_FactoryUsed.CurrentState != Factory.State.BuildingUnit)
                IsFinish = true;
        }
        else
            IsFinish = true;
    }
}