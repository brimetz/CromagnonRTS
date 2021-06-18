using UnityEngine;
using UnityEngine.AI;

public enum EPosture
{
    Passive,
    Aggressive,
    Repair,
}

public class Unit : BaseEntity
{
    [SerializeField]
    UnitDataScriptable UnitData = null;

    Transform BulletSlot;
    float LastActionDate = 0f;
    public BaseEntity EntityTarget = null;

    public TargetBuilding CaptureTarget = null;
    public NavMeshAgent NavMeshAgent;

    public Squad OwnSquad = null;
    public float MaxSpeed = 10;

    public float DistanceTask;
    public Vector3 TargetPos = Vector3.zero;
    public delegate void OnTaskDelegate();
    public OnTaskDelegate OnTask;

    public UnitDataScriptable GetUnitData { get { return UnitData; } }
    public int Cost { get { return UnitData.Cost; } }
    public int GetTypeId { get { return UnitData.TypeId; } }

    public StateMachine FSM = new StateMachine();
    public VisionEntity Vision;
    private EPosture m_Posture;
    public EPosture Posture
    {
        get { return m_Posture; }
        set
		{
            m_Posture = value;
            switch (m_Posture)
			{
                case EPosture.Passive:
                    FSM.SetState(new PassiveState(this));
                    return;
                case EPosture.Aggressive:
                    FSM.SetState(new AggressiveState(this));
                    return;
                case EPosture.Repair:
                    FSM.SetState(new RepairState(this));
                    return;
            }
		}
    }
    public BoxCollider BoxCollider = null;
    public CapsuleCollider CapsuleCollider = null;
    public Vector2 SizeCollider
    {
        get
        {
            if (BoxCollider != null)
                return BoxCollider.size.XZ();
            else
                return new Vector2(CapsuleCollider.radius, CapsuleCollider.radius);
        }
    }

    public TargetBuilding CaptureTargetTemp = null;
    public Factory AttackFactoryTemp = null;

    override public void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        base.Init(_team);
        Vision.Team = _team;
        Vision.Range = UnitData.RangeVision;
        HP = UnitData.MaxHP;
        HPBar.SetMaxHealth(UnitData.MaxHP);
        OnDeadEvent += Unit_OnDead;
    }
    void Unit_OnDead()
    {
        if (GetUnitData.DeathFXPrefab)
        {
            GameObject fx = Instantiate(GetUnitData.DeathFXPrefab, transform);
            fx.transform.parent = null;
        }

        StopCapture();

        if (OwnSquad != null)
        {
            OwnSquad.RemoveUT(this);
            OwnSquad = null;
        }

        Destroy(gameObject);
    }
    #region MonoBehaviour methods
    override protected void Awake()
    {
        base.Awake();

        NavMeshAgent = GetComponent<NavMeshAgent>();
        BulletSlot = transform.Find("BulletSlot");

        // fill NavMeshAgent parameters
        NavMeshAgent.speed = GetUnitData.Speed;
        NavMeshAgent.angularSpeed = GetUnitData.AngularSpeed;
        NavMeshAgent.acceleration = GetUnitData.Acceleration;

        MaxSpeed = NavMeshAgent.speed;

        if (GetComponent<BoxCollider>() != null)
            BoxCollider = GetComponent<BoxCollider>();
        else if (GetComponent<CapsuleCollider>() != null)
            CapsuleCollider = GetComponent<CapsuleCollider>();
    }
    override protected void Start()
    {
        // Needed for non factory spawned units (debug)
        if (!IsInitialized)
            Init(Team);

        base.Start();
        Posture = EPosture.Aggressive;
    }
    override protected void Update()
    {
        if (OwnSquad != null && OwnSquad.Units.Count == 0)
            OwnSquad = null;

        if (EntityTarget != null)
        {
            if (EntityTarget.GetTeam() != GetTeam())
                ComputeAttack();
            else
                ComputeRepairing();
        }

        FSM?.Update();
    }
    #endregion

    #region IRepairable
    override public bool NeedsRepairing()
    {
        return HP < GetUnitData.MaxHP;
    }
    override public void Repair(int amount)
    {
        HP = Mathf.Min(HP + amount, GetUnitData.MaxHP);
        base.Repair(amount);
    }
    override public void FullRepair()
    {
        Repair(GetUnitData.MaxHP);
    }
    #endregion

    #region Tasks methods : Moving, Capturing, Targeting, Attacking, Repairing ...

    public bool CanTask()
	{
        return CanTask(TargetPos, DistanceTask);
    }

    // Moving Task
    public void SetTargetPos(Vector3 targetPos, float distanceTask = 0f)
    {
        if (OnTask != null)
            OnTask = null;

        if (EntityTarget != null)
            EntityTarget = null;

        if (AttackFactoryTemp != null)
            AttackFactoryTemp = null;

        if (CaptureTargetTemp != null)
            CaptureTargetTemp = null;

        if (CaptureTarget != null)
            StopCapture();

        if (NavMeshAgent)
        {
            NavMeshAgent.SetDestination(targetPos);
            NavMeshAgent.isStopped = false;

            TargetPos = targetPos;
            DistanceTask = distanceTask;
        }
    }

    // Targetting Task - attack
    public void SetAttackTarget(BaseEntity target)
    {
        if (target == null)
            return;

        bool canTask = CanAttack(target);

        if (!canTask)
            SetTargetPos(target.transform.position, GetUnitData.AttackDistanceMax);

        OnTask = () =>
        {
            if (target == null)
                return;

            if (CaptureTarget != null)
                StopCapture();

            if (target.GetTeam() != GetTeam())
			{
                if (target is Factory)
                    AttackFactoryTemp = target as Factory;

                StartAttacking(target);
            }
        };

        if (canTask)
            OnTask();
    }

    // Targetting Task - capture
    public void SetCaptureTarget(TargetBuilding target)
    {
        bool canTask = CanCapture(target);

        if (!canTask)
            SetTargetPos(target.transform.position, GetUnitData.CaptureDistanceMax);

        OnTask = () =>
        {
            if (target == null)
                return;

            if (EntityTarget != null)
                EntityTarget = null;

            if (target.GetTeam() != GetTeam())
            {
                CaptureTargetTemp = target;
                StartCapture(target);
            }
        };

        if (canTask)
            OnTask();
    }

    // Targetting Task - repairing
    public void SetRepairTarget(BaseEntity entity)
    {
        if (entity == null)
            return;

        bool canTask = CanRepair(entity);

        if (!canTask)
            SetTargetPos(entity.transform.position, GetUnitData.RepairDistanceMax);

        OnTask = () =>
        {
            if (GetUnitData.CanRepair == false || entity == null)
                return;

            if (CaptureTarget != null)
                StopCapture();

            if (entity.GetTeam() == GetTeam())
                StartRepairing(entity);
        };

        if (canTask)
            OnTask();
    }

    public bool CanAttack(BaseEntity target)
    {
        if (target == null || GetUnitData == null || this == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.AttackDistanceMax * GetUnitData.AttackDistanceMax)
            return false;

        return true;
    }

    // Attack Task
    public void StartAttacking(BaseEntity target)
    {
        EntityTarget = target;
    }
    public void ComputeAttack()
    {
        if (CanAttack(EntityTarget) == false)
            return;

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        transform.LookAt(EntityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - LastActionDate) > UnitData.AttackFrequency)
        {
            LastActionDate = Time.time;
            // visual only ?
            if (UnitData.BulletPrefab)
            {
                GameObject newBullet = Instantiate(UnitData.BulletPrefab, BulletSlot);
                newBullet.transform.parent = null;
                newBullet.GetComponent<Bullet>().ShootToward(EntityTarget.transform.position - transform.position, this);
            }
            // apply damages
            int damages = Mathf.FloorToInt(UnitData.DPS * UnitData.AttackFrequency);
            EntityTarget.AddDamage(damages);
        }
    }
    public bool CanCapture(TargetBuilding target)
    {
        if (target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.CaptureDistanceMax * GetUnitData.CaptureDistanceMax)
            return false;

        return true;
    }

    public bool CanTask(Vector3 targetPos, float distanceMax)
    {
        // distance check
        if ((targetPos - transform.position).sqrMagnitude > distanceMax * distanceMax)
            return false;

        return true;
    }


    // Capture Task
    public void StartCapture(TargetBuilding target)
    {
        if (CanCapture(target) == false)
            return;

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        CaptureTarget = target;
        CaptureTarget.StartCapture(this);
    }
    public void StopCapture()
    {
        if (CaptureTarget == null)
            return;

        CaptureTarget.StopCapture(this);
        CaptureTarget = null;
    }

    // Repairing Task
    public bool CanRepair(BaseEntity target)
    {
        if (GetUnitData.CanRepair == false || target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.RepairDistanceMax * GetUnitData.RepairDistanceMax)
            return false;

        return true;
    }
    public void StartRepairing(BaseEntity entity)
    {
        if (GetUnitData.CanRepair)
        {
            EntityTarget = entity;
        }
    }

    // $$$ TODO : add repairing visual feedback
    public void ComputeRepairing()
    {
        if (CanRepair(EntityTarget) == false)
            return;

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        transform.LookAt(EntityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - LastActionDate) > UnitData.RepairFrequency)
        {
            LastActionDate = Time.time;

            // apply reparing
            int amount = Mathf.FloorToInt(UnitData.RPS * UnitData.RepairFrequency);
            EntityTarget.Repair(amount);
        }
    }
    #endregion

    public void UpdateSpeed(float newSpeed)
    {
        NavMeshAgent.speed = newSpeed;
    }

    public float GetMaxSpeed()
    {
        return MaxSpeed;
    }

    public bool IsInSquad()
    {
        return OwnSquad != null;
    }
}
