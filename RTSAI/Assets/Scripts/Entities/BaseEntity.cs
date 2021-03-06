using System;
using UnityEngine;

public abstract class BaseEntity : MonoBehaviour, ISelectable, IDamageable, IRepairable
{
    [SerializeField]
    protected ETeam Team;

    protected int HP = 0;
    protected Action OnHpUpdated;
    protected GameObject SelectedSprite = null;
    protected HPBar HPBar = null;
    protected bool IsInitialized = false;

    public Action OnDeadEvent;
    public bool IsSelected { get; protected set; }
    public bool IsAlive { get; protected set; }
    virtual public void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        Team = _team;

        IsInitialized = true;
    }

    public int GetHP()
    {
        return HP;
    }

    public Color GetColor()
    {
        return GameServices.GetTeamColor(GetTeam());
    }
    void UpdateHpUI()
    {
        if (HPBar != null)
            HPBar.SetHealth(HP);
    }

    #region ISelectable
    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (SelectedSprite == null)
            SelectedSprite = transform.Find("SelectedSprite")?.gameObject;

        SelectedSprite.SetActive(IsSelected);
    }
    public ETeam GetTeam()
    {
        return Team;
    }
    #endregion

    #region IDamageable
    public void AddDamage(int damageAmount)
    {
        if (IsAlive == false)
            return;

        HP -= damageAmount;

        OnHpUpdated?.Invoke();

        if (HP <= 0)
        {
            IsAlive = false;
            OnDeadEvent?.Invoke();
            Debug.Log("Entity " + gameObject.name + " died");
        }
    }
    public void Destroy()
    {
        AddDamage(HP);
    }
    #endregion

    #region IRepairable
    virtual public bool NeedsRepairing()
    {
        return true;
    }
    virtual public void Repair(int amount)
    {
        OnHpUpdated?.Invoke();
    }
    virtual public void FullRepair()
    {
    }
    #endregion

    #region MonoBehaviour methods
    virtual protected void Awake()
    {
        IsAlive = true;

        SelectedSprite = transform.Find("SelectedSprite")?.gameObject;
        SelectedSprite?.SetActive(false);

        HPBar = transform.GetComponentInChildren<HPBar>();

        OnHpUpdated += UpdateHpUI;
    }
    virtual protected void Start()
    {
        UpdateHpUI();
    }
    virtual protected void Update()
    {
    }
    #endregion
}
