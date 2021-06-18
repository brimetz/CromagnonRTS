using System.Collections.Generic;
using UnityEngine;

public sealed class AIController : UnitController
{
    [SerializeField]
    private Perception m_Perception = null;

    // Planner
    private StrategicalPlanner m_MacroPlanner = new StrategicalPlanner();
    private TacticalPlanner m_MicroPlanner = new TacticalPlanner();

    private StrategicalPlan m_CurrentPlan = null;

    [SerializeField]
    private EGoal m_LastCurrentGoal = EGoal.None;

    [SerializeField]
    private EGoal m_CurrentGoal = EGoal.None;

    [SerializeField]
    private float m_TimeSinceLastGetGoal = 0.0f;

    private List<Squad> m_Squads = new List<Squad>();

    #region MonoBehaviour methods

    protected override void Awake()
    {
        base.Awake();

        m_MacroPlanner.Init();
        m_MicroPlanner.Init();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (m_Perception != null)
        {
            m_TimeSinceLastGetGoal += Time.deltaTime;

            if (m_TimeSinceLastGetGoal > 0.5f)
            {
                m_TimeSinceLastGetGoal = 0.0f;
                m_LastCurrentGoal = m_CurrentGoal;
                m_CurrentGoal = m_Perception.Goal;
                
                if (m_CurrentGoal != m_LastCurrentGoal || 
                   m_CurrentPlan == null)
                {
                    m_CurrentPlan = m_MacroPlanner.GetPlan(m_CurrentGoal, this);
                }
                else if (m_CurrentGoal == m_LastCurrentGoal && m_CurrentPlan.IsFinish)
                {
                    m_CurrentGoal = EGoal.None;
                    m_Perception.Reset();
                }
            }
        }

        if (m_CurrentPlan != null)
            m_CurrentPlan.Execute();

        if (m_Squads.Count > 0)
        {          
            foreach (Squad Squad in m_Squads.ToArray())
            {
                if(Squad != null)
                {
                    if (Squad.DestroySquad)
                    {
                        m_Squads.Remove(Squad);
                        break;
                    }
                    else
                        Squad.Update(this);
                }
            }
        }
    }

    #endregion

    protected override bool RequestFactoryBuild(int factoryIndex, Vector3 buildPos, ref Factory FactoryBuilt)
    {
        if (FactoryList.Count > 0)
            SelectedFactory = FactoryList[0];
        else
            return false;

        int cost = SelectedFactory.GetFactoryCost(factoryIndex);
        if (TotalBuildPoints < cost)
            return false;

        // Check if positon is valid
        if (SelectedFactory.CanPositionFactory(factoryIndex, buildPos) == false)
            return false;

        Factory newFactory = SelectedFactory.StartBuildFactory(factoryIndex, buildPos);
        if (newFactory != null)
        {
            AddFactory(newFactory);
            TotalBuildPoints -= cost;

            FactoryBuilt = newFactory;

            return true;
        }
        return false;
    }

    public void UpdateSquad(Squad SquadToUpdate, ETacticalGoal Goal)
    {
        if (SquadToUpdate == null)
            return;

        Target NewTarget = new Target();

        if (!NewTarget.Init(this, SquadToUpdate, Goal))
            return;

        SquadToUpdate.m_TargetStruct = NewTarget;
        SquadToUpdate.CurrentGoal = Goal;

        m_MicroPlanner.GetPlan(Goal, SquadToUpdate, NewTarget);
    }

    public void BuildLightUT(int RandomIndex, ref Factory FactoryUsed)
    {
        foreach (Factory Fact in FactoryList)
        {
            if (Fact.FactoryData.Caption == "Light Factory")
            {
                Fact.RequestUnitBuild(RandomIndex);
                FactoryUsed = Fact;
            }
        }
    }

    public int GetCostLightUT(int RandomIndex)
    {
        foreach (Factory Fact in FactoryList)
        {
            if (Fact.FactoryData.Caption == "Light Factory")
            {
                return Fact.GetUnitCost(RandomIndex);
            }
        }
        return 0;
    }

    public void BuildHeavyUT(int RandomIndex, ref Factory FactoryUsed)
    {
        foreach (Factory Fact in FactoryList)
        {
            if (Fact.FactoryData.Caption == "Heavy Factory")
            {
                FactoryUsed = Fact;
                Fact.RequestUnitBuild(RandomIndex);
            }
        }
    }

    public int GetCostHeavyUT(int RandomIndex)
    {
        foreach (Factory Fact in FactoryList)
        {
            if (Fact.FactoryData.Caption == "Heavy Factory")
            {
                return Fact.GetUnitCost(RandomIndex);
            }
        }
        return 0;
    }

    public void BuildFactory(int FactoryIndex, Vector3 Location, ref Factory FactoryBuilt)
    {
        RequestFactoryBuild(FactoryIndex, Location, ref FactoryBuilt);
    }

    public int GetNumberLightFactory()
    {
        int Num = 0;
        foreach (Factory Fact in FactoryList)
        {
            if (Fact.FactoryData.Caption == "Light Factory")
            {
                Num++;
            }
        }
        return Num;
    }

    public int GetNumberHeavyFactory()
    {
        int Num = 0;
        foreach (Factory Fact in FactoryList)
        {
            if (Fact.FactoryData.Caption == "Heavy Factory")
            {
                Num++;
            }
        }
        return Num;
    }

    public int GetIdleUT()
    {
        int Num = 0;
        foreach (Unit Unit in UnitList)
        {
            if (!Unit.IsInSquad())
                Num++;
        }
        return Num;
    }

    public int GetIdleSquad()
    {
        int Num = 0;
        foreach (Squad Squad in m_Squads)
        {
            if (Squad.CurrentPlan == null || Squad.CurrentPlan.IsFinish)
                Num++;
        }
        return Num;
    }

    public int GetSquadOnTask(ETacticalGoal goal)
    {
        int nbSquad = 0;
        foreach (Squad squad in m_Squads)
        {
            if (squad.CurrentGoal == goal && squad.CurrentPlan != null && !squad.CurrentPlan.IsFinish)
                nbSquad++;
        }
        return nbSquad;
    }

    public int GetWaitingRecrueNumSquad()
    {
        int Num = 0;
        foreach (Squad Squad in m_Squads)
        {
            if (Squad.CurrentPlan.GetCurrentTask() is SearchRecrue)
                Num++;
        }
        return Num;
    }

    public int SearchSquadWaitingRecrue(int NbUnitAvailable, ref Squad SquadFound)
    {
        foreach (Squad Squad in m_Squads)
        {
            if (Squad.CurrentPlan.GetCurrentTask() is SearchRecrue)
            {
                int UnitNeeded = (int)Squad.NumUnitMax - Squad.Units.Count;
                if (UnitNeeded <= NbUnitAvailable - 1)
                {
                    SquadFound = Squad;
                    return UnitNeeded;
                }
            }
        }
        return 0;
    }

    public void SetCurrentPlan(StrategicalPlan NewPlan)
    {
        m_CurrentPlan = NewPlan;
    }

    public void AddSquad(Squad NewSquad)
    {
        m_Squads.Add(NewSquad);
    }

    public Unit GetIdleUTRandom()
    {
        foreach (Unit Unit in UnitList)
        {
            if (!Unit.IsInSquad())
            {
                return Unit;
            }
        }
        return null;
    }

    public Squad GetRandomIdleSquad(ETacticalGoal Goal)
    {
        if (Goal != ETacticalGoal.Defend)
        {
            foreach (Squad Squad in m_Squads)
            {
                if (Squad.CurrentPlan == null || Squad.CurrentPlan.IsFinish)
                {
                    if (Goal == ETacticalGoal.Repair && Squad.CanRepair())
                    {
                        return Squad;
                    }
                    else if (Goal == ETacticalGoal.Repair && !Squad.CanRepair())
                    {
                    }
                    else
                        return Squad;
                }
            }
        }
        else
        {
            foreach (Squad Squad in m_Squads)
            {
                if (Squad.CurrentPlan == null || 
                    Squad.CurrentPlan.IsFinish || 
                    Squad.CurrentGoal == ETacticalGoal.Secure ||
                    Squad.CurrentGoal == ETacticalGoal.Repair)
                {
                    Squad.ResetSecurisationOfBuilding();
                    return Squad;
                }
            }
        }
        return null;
    }

    public Vector3 GetLocationNewBuilding()
    {
        if (FactoryList.Count > 0)
            return FactoryList[0].transform.position + new Vector3(20, 0, 20) + new Vector3(Random.Range(-40, 15), 0, Random.Range(-40, 15));

        // Debug.LogError("AIController::GetLocationNewBuilding - AI doesn't have a factory!");
        return Vector3.zero;
    }

    public int NbSquadSearchingRecrue()
    {
        int nb = 0;
        foreach (Squad Squad in m_Squads)
        {
            if (Squad != null && Squad.CurrentPlan != null)
            {
                if (Squad.CurrentPlan.GetCurrentTask() is SearchRecrue)
                {
                    nb++;
                }
            }
        }
        return nb;
    }

    public Squad SearchSquadWaitingRecrue()
    {
        foreach (Squad Squad in m_Squads)
        {
            if (Squad != null && Squad.CurrentPlan.GetCurrentTask() is SearchRecrue)
            {
                return Squad;
            }
        }
        return null;
    }

    public void DisbandSquad(Squad Squad)
    {
        if (Squad != null)
        {
            Squad.RemoveAllUT();
            m_Squads.Remove(Squad);
        }
    }
}

