using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class Target
{
    private List<UnitContainer> m_UnitContainer = new List<UnitContainer>();
    private bool m_NeedToInit = true;

    private ETacticalGoal m_TacticalGoal;

    // Usefull when Goal = Conquer, Defend, Explore and Secure
    public Vector3 Location = Vector3.zero;

    // Usefull when Goal = Secure
    public List<Vector3> Waypoints = new List<Vector3>();

    // Usefull when Goal = Destroy
    public BaseEntity Entity = null;

    // Usefull when Goal = Conquer
    public TargetBuilding Building = null;
    // Usefull when Goal = Conquer
    public Factory FactoryBuilding = null;

    // Usefull when Squad need to wait
    public float Time = 0.0f;

    public int TaskToRepeat = 0;

    private Vector3 m_ExploreMaxPos = new Vector3(1f, 0f, 1f) * 440f;
    private Vector3 m_ExploreMinPos = new Vector3(1f, 0f, 1f) * 60f;

    public bool InitConquer(AIController AI)
    {
        if (AI.FactoryList.Count <= 0)
            return false;

        Vector3 basePosition = AI.FactoryList[0].transform.position;

        float minDistance = float.MaxValue;
        float currentDistance;
        int currentIndex = -1;
        
        // Take the nearest building 
        for (int i = 0; i < AI.TargetBuildingsSeen.Count; i++)
        {
            if (AI.TargetBuildingsSeen[i].GetTeam() != AI.GetTeam())
            {
                currentDistance = Vector3.Distance(basePosition, AI.TargetBuildingsSeen[i].transform.position);
                if (minDistance > currentDistance)
                {
                    minDistance = currentDistance;
                    currentIndex = i;
                }
            }
        }

        if (currentIndex >= 0)
        {
            Building = AI.TargetBuildingsSeen[currentIndex];
            Location = Building.transform.position;
            return true;
        }
        return false;
    }

    public bool InitDefend(AIController AI)
    {
        for (int i = 0; i < AI.FactoryList.Count; i++)
        {
            if (AI.FactoryList[i].IsUnderAttack)
            {
                Location = AI.FactoryList[i].transform.position;
                CreateWaypoint();
                return true;
            }
        }
        return false;
    }

    public bool InitDestroy(AIController AI)
    {
        if (AI.FactoriesEnemySeen.Count >= 1)
        {
            int randFactory = UnityEngine.Random.Range(0, AI.FactoriesEnemySeen.Count - 1);
            Entity = AI.FactoriesEnemySeen[randFactory];
            Location = AI.FactoriesEnemySeen[randFactory].transform.position;
            return true;
        }
        return false;
    }

    public bool InitExplore(AIController AI, Squad currentSquad)
    {
        if (currentSquad == null || currentSquad.Units.Count == 0 || currentSquad.Units[0] == null)
            return false;

        int searchNumber = 5;

        NavMeshPath navMeshPath = new NavMeshPath();
		Vector3 destination = new Vector3();

        do
        {
            --searchNumber;
            destination.Random(m_ExploreMinPos, m_ExploreMaxPos);
            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 100f, 1))
            {
                destination = hit.position;
                currentSquad.Units[0].NavMeshAgent.CalculatePath(destination, navMeshPath);
            }
        }
        while (navMeshPath.status != NavMeshPathStatus.PathComplete ||
        (searchNumber > 0 ? AI.FOWManager.VisionSystem.WasVisible(1 << (int)AI.GetTeam(), destination.XZ()) :
                            AI.FOWManager.VisionSystem.IsVisible(1 << (int)AI.GetTeam(), destination.XZ())));

        Location = destination;
        Debug.Log("Target::InitExplore - Location is " + destination);

        return true;
    }

    public bool InitFlee(AIController AI)
    {
        if (AI.FactoryList.Count > 0)
        {
            int randFactory = UnityEngine.Random.Range(0, AI.FactoryList.Count - 1);
            Location = AI.FactoryList[randFactory].transform.position;
            return true;
        }
        else
		{
            return false;
		}
    }

    public void ChangeOffset(ref float offsetX, ref float offsetZ, int index)
    {
        if (index == 1 || index == 3)
            offsetX *= -1;
        else if (index == 2)
            offsetZ *= -1;
    }

    public void CreateWaypoint()
    {
        int nbWaypoint = UnityEngine.Random.Range(2, 5);
        float offsetX = -20f, offsetZ = -20f;

        for (int i = 0; i < nbWaypoint; i++)
        {
            ChangeOffset(ref offsetX, ref offsetZ, i);
            Vector3 checkpoint = Location;
            checkpoint.x += offsetX;
            checkpoint.z += offsetZ;
            Waypoints.Add(checkpoint);
        }
    }

    public bool InitSecure(AIController AI)
    {
        for (int i = 0; i < AI.TargetBuildingsSeen.Count; i++)
        {
            if (AI.TargetBuildingsSeen[i].GetTeam() == AI.GetTeam()
                && !AI.TargetBuildingsSeen[i].secure)
            {
                Building = AI.TargetBuildingsSeen[i];
                Location = AI.TargetBuildingsSeen[i].transform.position;
                Building.secure = true;
                CreateWaypoint();
                return true;
            }
        }
      
        for (int i = 0; i < AI.FactoryList.Count; i++)
        {
            if (!AI.FactoryList[i].secure)
            {
                FactoryBuilding = AI.FactoryList[i];
                Location = AI.FactoryList[i].transform.position;
                FactoryBuilding.secure = true;
                CreateWaypoint();
                return true;
            }
        }
        return false;
    }

    bool InitRepair(AIController AI)
    {
        for (int i = 0; i < AI.FactoryList.Count; i++)
        {
            if (AI.FactoryList[i].GetHP() < AI.FactoryList[i].GetFactoryData.MaxHP)
            {
                Location = AI.FactoryList[i].transform.position;
                Entity = AI.FactoryList[i].GetComponent<BaseEntity>();
                return true;
            }
        }

        for (int i = 0; i < AI.UnitList.Count; i++)
        {
            if (AI.UnitList[i].GetHP() < AI.UnitList[i].GetUnitData.MaxHP)
            {
                Location = AI.UnitList[i].transform.position;
                Entity = AI.UnitList[i].GetComponent<BaseEntity>();
                return true;
            }
        }
        return true;
    }

    public bool Init(AIController AI, Squad CurrentSquad, ETacticalGoal tacticalGoal)
    {
        if (m_NeedToInit)
            InitUnitContainer();

        m_TacticalGoal = tacticalGoal;
		return m_TacticalGoal switch
		{
			ETacticalGoal.Conquer => InitConquer(AI),
			ETacticalGoal.Defend => InitDefend(AI),
			ETacticalGoal.Destroy => InitDestroy(AI),
			ETacticalGoal.Explore => InitExplore(AI, CurrentSquad),
			ETacticalGoal.Flee => InitFlee(AI),
			ETacticalGoal.Secure => InitSecure(AI),
			ETacticalGoal.Repair => InitRepair(AI),
			_ => false,
		};
	}

    private void InitUnitContainer()
    {
        m_NeedToInit = false;
        m_UnitContainer.Add(new UnitContainer(EUnit.HeavyTankB, 60));
        m_UnitContainer.Add(new UnitContainer(EUnit.HeavyTankA, 50));
        m_UnitContainer.Add(new UnitContainer(EUnit.Quad, 20));
        m_UnitContainer.Add(new UnitContainer(EUnit.Trike, 15));
        m_UnitContainer.Add(new UnitContainer(EUnit.Infantry, 10));
        Waypoints.Clear();
    }
}
