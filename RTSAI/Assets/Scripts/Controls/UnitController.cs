using System;
using System.Collections.Generic;
using UnityEngine;

// Points system for units creation (Ex : light units = 1 pt, medium = 2pts, heavy = 3 pts)
// Max points can be increased by capturing TargetBuilding entities
public class UnitController : MonoBehaviour
{
    [SerializeField]
    protected ETeam Team;
    public ETeam GetTeam() { return Team; }

    [SerializeField]
    protected int StartingBuildPoints = 15;

    protected int _TotalBuildPoints = 0;
    public int TotalBuildPoints
    {
        get { return _TotalBuildPoints; }
        set
        {
            Debug.Log("TotalBuildPoints updated");
            _TotalBuildPoints = value;
            OnBuildPointsUpdated?.Invoke();
        }
    }

    protected int _CapturedTargets = 0;
    public int CapturedTargets
    {
        get { return _CapturedTargets; }
        set
        {
            _CapturedTargets = value;
            OnCaptureTarget?.Invoke();
        }
    }

    protected Transform TeamRoot = null;
    public Transform GetTeamRoot() { return TeamRoot; }

    public List<Unit> UnitList = new List<Unit>();
    protected List<Unit> SelectedUnitList = new List<Unit>();
    public List<Factory> FactoryList = new List<Factory>();
    protected Factory SelectedFactory = null;

    public List<Unit> UnitsEnemySeen = new List<Unit>();
    public List<Factory> FactoriesEnemySeen = new List<Factory>();
    public List<TargetBuilding> TargetBuildingsSeen = new List<TargetBuilding>();

    protected FogOfWarManager m_FOWManager = null;
    public FogOfWarManager FOWManager	
    {

        get { return m_FOWManager; }
	}

    // events
    protected Action OnBuildPointsUpdated;
    protected Action OnCaptureTarget;

    #region Unit methods
    protected void UnselectAllUnits()
    {
        foreach (Unit unit in SelectedUnitList)
            unit.SetSelected(false);
        SelectedUnitList.Clear();
    }
    protected void SelectAllUnits()
    {
        foreach (Unit unit in UnitList)
            unit.SetSelected(true);

        SelectedUnitList.Clear();
        SelectedUnitList.AddRange(UnitList);
    }
    protected void SelectAllUnitsByTypeId(int typeId)
    {
        UnselectCurrentFactory();
        UnselectAllUnits();
        SelectedUnitList = UnitList.FindAll(delegate (Unit unit)
            {
                return unit.GetTypeId == typeId;
            }
        );
        foreach (Unit unit in SelectedUnitList)
        {
            unit.SetSelected(true);
        }
    }
    protected void SelectUnitList(List<Unit> units)
    {
        foreach (Unit unit in units)
            unit.SetSelected(true);
        SelectedUnitList.AddRange(units);
    }
    protected void SelectUnitList(Unit [] units)
    {
        foreach (Unit unit in units)
            unit.SetSelected(true);
        SelectedUnitList.AddRange(units);
    }
    protected void SelectUnit(Unit unit)
    {
        unit.SetSelected(true);
        SelectedUnitList.Add(unit);
    }
    protected void UnselectUnit(Unit unit)
    {
        unit.SetSelected(false);
        SelectedUnitList.Remove(unit);
    }
    virtual public void AddUnit(Unit unit)
    {
        unit.OnDeadEvent += () =>
        {
            TotalBuildPoints += unit.Cost;
            if (unit.IsSelected)
            {
                SelectedUnitList.Remove(unit);
            }
            UnitList.Remove(unit);
            //UnitsSeen.Remove(unit);
            GameServices.GetControllerByTeam(Team.GetEnemyTeam()).RemoveUnitSeen(unit);
        };
        UnitList.Add(unit);
        //UnitsSeen.Add(unit);
    }
    public void CaptureTarget(int points)
    {
        Debug.Log("CaptureTarget");
        TotalBuildPoints += points;
        CapturedTargets++;
    }
    public void LoseTarget(int points)
    {
        TotalBuildPoints -= points;
        CapturedTargets--;
    }
    #endregion

    #region Factory methods
    protected void AddFactory(Factory factory)
    {
        if (factory == null)
        {
            Debug.LogWarning("Trying to add null factory");
            return;
        }

        factory.OnDeadEvent += () =>
        {
            TotalBuildPoints += factory.Cost;
            if (factory.IsSelected)
                SelectedFactory = null;
            FactoryList.Remove(factory);
            //FactoriesSeen.Remove(factory);
            GameServices.GetControllerByTeam(Team.GetEnemyTeam()).RemoveFactorySeen(factory);
        };
        FactoryList.Add(factory);
        //FactoriesSeen.Add(factory);
    }
    virtual protected void SelectFactory(Factory factory)
    {
        if (factory == null || factory.IsUnderConstruction)
            return;

        SelectedFactory = factory;
        SelectedFactory.SetSelected(true);
        UnselectAllUnits();
    }
    virtual protected void UnselectCurrentFactory()
    {
        if (SelectedFactory != null)
            SelectedFactory.SetSelected(false);
        SelectedFactory = null;
    }
    protected bool RequestUnitBuild(int unitMenuIndex)
    {
        if (SelectedFactory == null)
            return false;

        return SelectedFactory.RequestUnitBuild(unitMenuIndex);
    }
    protected virtual bool RequestFactoryBuild(int factoryIndex, Vector3 buildPos, ref Factory FactoryBuilt)
    {
        if (SelectedFactory == null)
            return false;

        int cost = SelectedFactory.GetFactoryCost(factoryIndex);
        if (TotalBuildPoints < cost)
            return false;

        // Check if positon is valid
        if (SelectedFactory.CanPositionFactory(factoryIndex, buildPos) == false)
            return false;

        // Check if position was visible
        if (m_FOWManager.VisionSystem.WasVisible(1 << (int)Team, buildPos.XZ()) == false)
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
    #endregion

    #region MonoBehaviour methods
    virtual protected void Awake()
    {
        string rootName = Team.ToString() + "Team";
        TeamRoot = GameObject.Find(rootName)?.transform;
        if (TeamRoot)
            Debug.LogFormat("TeamRoot {0} found !", rootName);

        m_FOWManager = FindObjectOfType<FogOfWarManager>();
    }
    virtual protected void Start ()
    {
        CapturedTargets = 0;
        TotalBuildPoints = StartingBuildPoints;

        // get all team factory already in scene
        Factory [] allFactories = FindObjectsOfType<Factory>();
        foreach(Factory factory in allFactories)
        {
            if (factory.GetTeam() == GetTeam())
            {
                AddFactory(factory);
            }
        }

        Debug.Log("found " + FactoryList.Count + " factory for team " + GetTeam().ToString());
    }
    virtual protected void Update ()
    {
		
	}
    #endregion

    public void AddUnitSeen(Unit unit)
    {
        if (!UnitsEnemySeen.Contains(unit))
            UnitsEnemySeen.Add(unit);
    }

    public void RemoveUnitSeen(Unit unit)
    {
        if (UnitsEnemySeen.Contains(unit))
            UnitsEnemySeen.Remove(unit);
    }

    public void AddFactorySeen(Factory factory)
    {
        if (!FactoriesEnemySeen.Contains(factory))
            FactoriesEnemySeen.Add(factory);
    }

    public void RemoveFactorySeen(Factory factory)
    {
        if (FactoriesEnemySeen.Contains(factory))
            FactoriesEnemySeen.Remove(factory);
    }

    public void AddTargetBuildingsSeen(TargetBuilding targetBuilding)
    {
        if (!TargetBuildingsSeen.Contains(targetBuilding))
            TargetBuildingsSeen.Add(targetBuilding);
    }
}
