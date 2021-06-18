using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetBuilding : MonoBehaviour
{
    [SerializeField]
    float CaptureGaugeStart = 100f;
    [SerializeField]
    float CaptureGaugeSpeed = 1f;
    [SerializeField]
    int BuildPoints = 5;
    [SerializeField]
    Material BlueTeamMaterial = null;
    [SerializeField]
    Material RedTeamMaterial = null;
	[SerializeField]
	Material NeutralMaterial = null;
	MeshRenderer BuildingMeshRenderer = null;
    Image GaugeImage;

    [SerializeField]
    int[] TeamScore;

    List<Unit> CapturingUnits = new List<Unit>();

    float CaptureGaugeValue;
    public bool secure = false;
    ETeam OwningTeam = ETeam.Neutral;

    ETeam CapturingTeam = ETeam.Neutral;
    public ETeam GetTeam() { return OwningTeam; }

    [SerializeField]
    private Image m_ImageMinimap;
    public VisionEntity Vision;
    private ETeam m_TeamForBlueTeam;
    private ETeam m_TeamForRedTeam;

	#region MonoBehaviour methods
	void Awake()
	{
        BuildingMeshRenderer = GetComponentInChildren<MeshRenderer>();
        GaugeImage = GetComponentInChildren<Image>();

        m_TeamForBlueTeam = ETeam.Neutral;
        m_TeamForRedTeam = ETeam.Neutral;
    }
	void Start()
    {
        if (GaugeImage)
            GaugeImage.fillAmount = 0f;
        CaptureGaugeValue = CaptureGaugeStart;
        TeamScore = new int[2];
        TeamScore[0] = 0;
        TeamScore[1] = 0;
    }
    void Update()
    {
        if (CapturingTeam == OwningTeam || CapturingTeam == ETeam.Neutral)
            return;

        CaptureGaugeValue -= TeamScore[(int)CapturingTeam] * CaptureGaugeSpeed * 60f * Time.deltaTime;

        GaugeImage.fillAmount = 1f - CaptureGaugeValue / CaptureGaugeStart;

        if (CaptureGaugeValue <= 0f)
        {
            CaptureGaugeValue = 0f;
            OnCaptured(CapturingTeam);
        }
    }
    #endregion

    #region Capture methods
    public void StartCapture(Unit unit)
    {
        if (unit == null)
            return;

        if (CapturingUnits.Contains(unit))
            return;

        CapturingUnits.Add(unit);

        TeamScore[(int)unit.GetTeam()] += unit.Cost;

        if (CapturingTeam == ETeam.Neutral)
        {
            if (TeamScore[(int)GameServices.GetOpponent(unit.GetTeam())] == 0)
            {
                CapturingTeam = unit.GetTeam();
                GaugeImage.color = GameServices.GetTeamColor(CapturingTeam);
            }
        }
        else
        {
            if (TeamScore[(int)GameServices.GetOpponent(unit.GetTeam())] > 0)
                ResetCapture();
        }
    }
    public void StopCapture(Unit unit)
    {
        if (unit == null)
            return;

        if (!CapturingUnits.Contains(unit))
            return;

        CapturingUnits.Remove(unit); 

        TeamScore[(int)unit.GetTeam()] -= unit.Cost;
        if (TeamScore[(int)unit.GetTeam()] == 0)
        {
            ETeam opponentTeam = GameServices.GetOpponent(unit.GetTeam());
            if (TeamScore[(int)opponentTeam] == 0)
                ResetCapture();
            else
            {
                CapturingTeam = opponentTeam;
                GaugeImage.color = GameServices.GetTeamColor(CapturingTeam);
            }
        }
    }
    void ResetCapture()
    {
        CaptureGaugeValue = CaptureGaugeStart;
        CapturingTeam = ETeam.Neutral;
        GaugeImage.fillAmount = 0f;
    }
    void OnCaptured(ETeam newTeam)
    {
        bool buildingNotNeutral = OwningTeam != ETeam.Neutral;
        Debug.Log("target captured by " + newTeam.ToString());
        if (OwningTeam != newTeam)
        {
            UnitController teamController = GameServices.GetControllerByTeam(newTeam);
            if (teamController != null)
                teamController.CaptureTarget(BuildPoints);

            Vision.Team = newTeam;

            if (buildingNotNeutral)
            {
                // remove points to previously owning team
                teamController = GameServices.GetControllerByTeam(OwningTeam);
                if (teamController != null)
                    teamController.LoseTarget(BuildPoints);
            }
        }

        for(int i = 0; i < TeamScore.Length; i++)
            TeamScore[i] = 0;

        for (int i = 0; i < CapturingUnits.Count; i++)
        {
            CapturingUnits[i].CaptureTarget = null;
            CapturingUnits[i].CaptureTargetTemp = null;
        }

        CapturingUnits.Clear();
        CapturingTeam = ETeam.Neutral;

        CaptureGaugeValue = CaptureGaugeStart;

        GaugeImage.fillAmount = 0f;
        OwningTeam = newTeam;

        if (buildingNotNeutral)
            UpdateTeam(OwningTeam.GetEnemyTeam());
    }

    public void UpdateTeam(ETeam team)
    {
        if (team == ETeam.Blue)
            m_TeamForBlueTeam = OwningTeam;
        else if (team == ETeam.Red)
            m_TeamForRedTeam = OwningTeam;
    }

    public void UpdateTeamMaterial(ETeam team)
    {
        if (team == ETeam.Blue && BuildingMeshRenderer.material != GetMaterial(m_TeamForBlueTeam))
        {
            BuildingMeshRenderer.material = GetMaterial(m_TeamForBlueTeam);
            m_ImageMinimap.color = GetColor(m_TeamForBlueTeam);
        }
        else if (team == ETeam.Red && BuildingMeshRenderer.material != GetMaterial(m_TeamForRedTeam))
        {
            BuildingMeshRenderer.material = GetMaterial(m_TeamForRedTeam);
            m_ImageMinimap.color = GetColor(m_TeamForRedTeam);
        }
    }

    public Material GetMaterial(ETeam team)
	{
		return team switch
		{
			ETeam.Blue => BlueTeamMaterial,
			ETeam.Red => RedTeamMaterial,
			ETeam.Neutral => NeutralMaterial,
			_ => null,
		};
	}

    public Color GetColor(ETeam team)
    {
        return team switch
        {
            ETeam.Blue => Color.blue,
            ETeam.Red => Color.red,
            ETeam.Neutral => Color.green,
            _ => Color.grey,
        };
    }
}
#endregion
