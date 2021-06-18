using UnityEngine;

public class CreateLightFactory : StrategicalTask
{
    public Vector3 Location = Vector3.zero;
    private Factory m_FactoryBuilt = null;

    public override void Init()
    {
    }

    public override bool CheckPreCondition(AIController AI, EGoal goal, int[] currentState, int[] finalState)
    {
        // if LightFactory number is below the LightFactory Final state AND if ressoucres > LightFactory Price
        if (currentState[1] < finalState[1] && currentState[0] >= 10)
            return true;
        return false;
    }

    public override void PostCondition(ref int[] currentState, int[] finalState)
    {
        // Add One LightFactory and remove the LightFactory price from ressources
        currentState[0] -= 10;
        currentState[1] += 1;
    }

    public override void Execute(AIController AI)
    {
        AI.BuildFactory(0, Location, ref m_FactoryBuilt);
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
