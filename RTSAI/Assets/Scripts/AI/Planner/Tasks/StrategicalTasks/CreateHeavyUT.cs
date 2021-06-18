using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateHeavyUT : StrategicalTask
{
    public int NbUnit = 1;

    private int m_UsedRessources = 0;
    private Factory m_FactoryUsed = null;

    private int m_IndexUT = Random.Range(0, 3);

    public override void Init()
    {
    }

    public override bool CheckPreCondition(AIController AI, EGoal goal, int[] currentState, int[] finalState)
    {
        // If ressources >= a 5 And There is one heavy factory
        if (currentState[0] >= 5 && currentState[2] > 0)
        {
            for (int i = 0; i < NbUnit; i++)
                m_UsedRessources += AI.GetCostHeavyUT(m_IndexUT);

            return true;
        }

        return false;
    }

    public override void PostCondition(ref int[] currentState, int[] finalState)
    {
        // Remove the nb ressources used to create UT and Add the nb of created unit in IdleUnitNb
        currentState[0] -= m_UsedRessources;
        currentState[3] += NbUnit;
    }

    public override void Execute(AIController AI)
    {
        for (int i = 0; i < NbUnit; i++)
            AI.BuildHeavyUT(m_IndexUT, ref m_FactoryUsed);
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
