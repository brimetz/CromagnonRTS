using UnityEngine;

public class CreateHeavyFactory : StrategicalTask
{
    public Vector3 Location = Vector3.zero;
    private Factory m_FactoryBuilt = null;

    public override void Init()
    {
    }

    public override bool CheckPreCondition(AIController AI, EGoal goal, int[] currentState, int[] finalState)
    {
        // if HeavyFactory number is below the heavyFactory Final state AND if ressoucres > FactoryHeavy Price
        if (currentState[2] < finalState[2] && currentState[0] >= 15)
            return true;
        return false;
    }

    public override void PostCondition(ref int[] currentState, int[] finalState)
    {
        // Add One HeavyFactory and remove the heavy factory price from ressources
        currentState[0] -= 15;
        currentState[2] += 1;
    }

    public override void Execute(AIController AI)
    {
        AI.BuildFactory(1, Location, ref m_FactoryBuilt);
    }

    public override void Update(AIController AI)
    {
        if (m_FactoryBuilt != null)
        {
            if (m_FactoryBuilt.CurrentState != Factory.State.UnderConstruction)
                IsFinish = true;
        }
        else
            IsFinish = true;
    }
}
