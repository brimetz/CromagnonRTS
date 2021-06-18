using System.Collections.Generic;
using UnityEngine;

public enum ETacticalGoal
{
    Conquer,
    Defend,
    Destroy,
    Explore,
    Secure,
    Repair,
    Flee
}

public enum EFormation
{
    Unstructured,
    Line, 
    Column,
    Square,
    Triangle
}

[System.Serializable]
public class Squad : ISelectable
{
    [SerializeField]
    public Target m_TargetStruct = new Target();

    [SerializeField]
    protected ETeam Team;

    public bool IsSelected { get; protected set; }

    public List<Unit> Units = new List<Unit>();
    public uint NumUnitMax = 4;

    public bool DestroySquad = false;

    public EFormation currentFormation = EFormation.Unstructured;

    public TacticalPlan CurrentPlan = new TacticalPlan();
    public ETacticalGoal CurrentGoal = ETacticalGoal.Explore;

    public int Count => Units.Count;

    // Player Squad
    Vector3 m_LocationGoal = Vector3.zero;

    public void Update(AIController OwnAI)
    {
        if (CurrentPlan == null)
            return;

        CurrentPlan.Execute();

        if (CurrentPlan.TaskCount == 0)
            CurrentPlan = null;


        if (NeedToFlee())
        {
            ResetSecurisationOfBuilding();

            if (CurrentGoal != ETacticalGoal.Flee)
                OwnAI.UpdateSquad(this, ETacticalGoal.Flee);
        }

        if (Units.Count == 0)
        {
            DestroySquad = true;
        }
    }

    public void ResetSecurisationOfBuilding()
    {
        if (m_TargetStruct.Building)
            m_TargetStruct.Building.secure = false;
        else if (m_TargetStruct.FactoryBuilding)
            m_TargetStruct.FactoryBuilding.secure = false;
    }

    public int GetNbLine()
    {
        int i = 2;
        while (true)
        {
            if (i * i >= Units.Count)
                return i;
            i++;
        }
    }

    public void SquareFormation(Vector3 LocationGoal, Vector3 Direction)
    {
        int nbLine = GetNbLine();
        float offsetX = 0f;
        float offsetZ = 0f;
        int troopOnLine = 0;
        Vector3 GoalPos = LocationGoal;


        for (int i = 0; i < Units.Count; i++)
        {
            if (Direction == Vector3.zero)
                GoalPos = GoalPos + (offsetX * Vector3.right);
            else
                GoalPos = GoalPos + (offsetX * Vector3.Cross(Direction, Vector3.up));
            Units[i].SetTargetPos(GoalPos);
            offsetX = 6.5f;
            troopOnLine++;
            if (troopOnLine == nbLine)
            {
                troopOnLine = 0;
                offsetX = 0f;
                offsetZ += 7.5f;

                if (Direction == Vector3.zero)
                    GoalPos = LocationGoal + (offsetZ * Vector3.forward);
                else
                    GoalPos = LocationGoal + (offsetZ * Direction);
            }
        }
    }

    //Set Offset of each unit in the Triangle Formation Squad
    public void SetPosUnitTriangle(ref Vector3 GoalPos, Vector3 LocationGoal, Vector3 Direction, float offsetZ, float offset, int index)
    {
        if (Direction == Vector3.zero)
        {
            GoalPos = LocationGoal - (offsetZ * Vector3.forward);
            GoalPos = GoalPos + (offset * Vector3.right);
        }
        else
        {
            GoalPos = LocationGoal + (offsetZ * Direction);
            GoalPos = GoalPos + (offset * Vector3.Cross(Direction, Vector3.up));
        }
    }

    public void TriangleFormation(Vector3 LocationGoal, Vector3 Direction)
    {
        float offsetZ = 7.5f;
        float offset = -3.25f;
        float offsetX = 0f;
        int nbTroops = 0, troopOnLine = 2;
        Vector3 GoalPos = LocationGoal;


        Units[0].SetTargetPos(GoalPos);
        SetPosUnitTriangle(ref GoalPos, LocationGoal, Direction, offsetZ, offset, 0);

        for (int i = 1; i < Units.Count; i++)
        {
            if (Direction == Vector3.zero)
                GoalPos = GoalPos + (offsetX * Vector3.right);
            else
                GoalPos = GoalPos + (offsetX * Vector3.Cross(Direction, Vector3.up));

            Units[i].SetTargetPos(GoalPos);

            nbTroops++;
            offsetX = 6.5f;
            if (nbTroops == troopOnLine)
            {
                offset -= 3.25f;
                offsetZ += 7.5f;
                offsetX = 0f;
                troopOnLine++;
                nbTroops = 0;
                SetPosUnitTriangle(ref GoalPos, LocationGoal, Direction, offsetZ, offset, i);
            }
        }
    }

    //Set Target Pos of each unit in the Line/Column/Unstructured Formation Squad
    public void SetTargetPos(Vector3 LocationGoal, Vector3 Direction, int i, int type, float offset)
    {

        //Unit Pos in Line
        if (type == 0)
        {
            if (Direction == Vector3.zero)
                Units[i].SetTargetPos(LocationGoal + offset * i * Vector3.right);
            else
                Units[i].SetTargetPos(LocationGoal + offset * i * Vector3.Cross(Direction, Vector3.up));
        }
        //Unit Pos in Column
        else if (type == 1)
        {
            if (Direction == Vector3.zero)
                Units[i].SetTargetPos(LocationGoal + offset * i * Vector3.forward);
            else
                Units[i].SetTargetPos(LocationGoal + offset * i * Direction);
        }
        //Unit Pos in Diagonal
        else if (type == 2)
        {
            if (Direction == Vector3.zero)
                Units[i].SetTargetPos(LocationGoal + (offset * i * Vector3.forward) + (offset * i * Vector3.right));
            else
                Units[i].SetTargetPos(LocationGoal + (offset * i * Direction) + (offset * i * Vector3.Cross(Direction, Vector3.up)));
        }
    }

    public void UnstructuredFormation(Vector3 LocationGoal, Vector3 Direction)
    {
        int rand = 0;
        float offset = 2f;
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i])
            {
                rand = Random.Range(0, 50);
                if (rand % 2 == 0)
                    SetTargetPos(LocationGoal, Direction, i, 0, offset);
                else if (rand % 3 == 0)
                    SetTargetPos(LocationGoal, Direction, i, 2, offset);
                else if (rand % 2 == 1)
                    SetTargetPos(LocationGoal, Direction, i, 1, offset);

            }
        }
    }

    public void UpdateOffset()
    {
        if (Units.Count == 0)
            return;

        if (m_LocationGoal == Vector3.zero && Units[0])
            m_LocationGoal = Units[0].transform.position;

        Vector3 Direction = Vector3.zero;

        if (Units[0].TargetPos != Vector3.zero)
        {
            Direction = Units[0].TargetPos - m_LocationGoal;
            Direction = Direction.normalized;
        }

        switch (currentFormation)
        {
            case EFormation.Unstructured:
                UnstructuredFormation(m_LocationGoal, Direction);
                break;
            case EFormation.Line:
                for (int i = 0; i < Units.Count; i++)
                {
                    if (Units[i])
                        SetTargetPos(m_LocationGoal, Direction, i, 0, 6.5f);
                }
                break;
            case EFormation.Column:
                for (int i = 0; i < Units.Count; i++)
                {
                    if (Units[i])
                        SetTargetPos(m_LocationGoal, Direction, i, 1, 7.5f);
                }
                break;
            case EFormation.Square:
                SquareFormation(m_LocationGoal, Direction);
                break;
            case EFormation.Triangle:
                TriangleFormation(m_LocationGoal, Direction);
                break;
            default:
                break;
        }
    }

    public void AddUT(Unit Unit)
    {
        if (Unit == null)
            return;

        Units.Add(Unit);

        if (Unit.OwnSquad != null)
            Unit.OwnSquad.RemoveUT(Unit);

        Unit.OwnSquad = this;
    }

    public void RemoveUT(Unit Unit)
    {
        Units.Remove(Unit);
    }

    public bool IsFull()
    {
        return Units.Count == NumUnitMax;
    }

    public void SetWaitingForRecrue()
    {

    }

    #region ISelectable
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i].IsAlive)
                Units[i].SetSelected(selected);
        }
    }
    public ETeam GetTeam()
    {
        return Team;
    }

    public void SetTeam(ETeam t)
    {
        Team = t;
    }
    #endregion

#region Targetting and movement
    public void SetAttackTarget(BaseEntity Other)
    {
        foreach (Unit Unit in Units)
            Unit.SetAttackTarget(Other);
    }

    public void SetRepairTarget(BaseEntity Other)
    {
        foreach (Unit Unit in Units)
            Unit.SetRepairTarget(Other);
    }

    public void SetCaptureTarget(TargetBuilding Target)
    {
        foreach (Unit Unit in Units)
            Unit.SetCaptureTarget(Target);
    }

    public void SetPositionTarget(Vector3 Position)
    {
        if (Units.Count == 0)
            return; 

        m_LocationGoal = Position;

        float MinSpeed = 100000.0f;
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i] && Units[i].GetMaxSpeed() < MinSpeed)
                MinSpeed = Units[i].GetMaxSpeed();
        }

        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i])
                Units[i].UpdateSpeed(MinSpeed);
        }

        UpdateOffset();
    }

    public bool IsArrived()
    {
        if (Units.Count > 0 && Units[0] && Vector3.Distance(Units[0].transform.position, m_LocationGoal) < 15)
            return true;
        return false;
    }

#endregion

    public void SetPosture(EPosture posture)
    {
        if (posture == EPosture.Repair)
        {
            foreach (Unit Unit in Units)
                if (Unit.GetUnitData.CanRepair)
                    Unit.Posture = posture;
        }
        else
        {
            foreach (Unit Unit in Units)
                Unit.Posture = posture;
        }
    }

    public int GetPosture()
	{
        int result = 0;
        for (int i = 0; i < Units.Count; ++i)
            result |= 1 << (int)Units[i].Posture;

        return result;
    }

    public bool CanRepair()
	{
        foreach (Unit unit in Units)
            if (unit.GetUnitData.CanRepair)
                return true;
        return false;
	}

    bool NeedToFlee()
    {
        if (Units.Count <= 2 && Units.Count != 0)
            return true;

        return false;
    }

    public void RemoveAllUT()
    {
        Units.Clear();
    }
}
