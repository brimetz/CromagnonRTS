using UnityEngine;

public class TopCamera : MonoBehaviour
{
    Camera m_Camera;

    public int MoveSpeed = 5;
    public int KeyboardSpeedModifier = 20;
    public int ZoomSpeed = 100;
    public float ZoomSizeMin = 5f;
    public float ZoomSizeMax = 100f;

    public AnimationCurve MoveSpeedFromZoomCurve = new AnimationCurve();
    public float TerrainBorder = 100f;
    [Tooltip("Set to false for debug camera movement")]
    public bool EnableMoveLimits = true;

    public Vector3 Move = Vector3.zero;
    public Vector3 TerrainSize = Vector3.zero;

    #region Camera movement methods
    public void Zoom(float value)
    {
        if (value < 0f)
        {
            Move.y += ZoomSpeed * Time.deltaTime;
        }
        else if (value > 0f)
        {
            Move.y -= ZoomSpeed * Time.deltaTime;
        }

        /*if (value < 0f)
            m_Camera.orthographicSize += ZoomSpeed * Time.deltaTime;
        else if (value > 0f)
            m_Camera.orthographicSize -= ZoomSpeed * Time.deltaTime;*/

        if (EnableMoveLimits)
            m_Camera.orthographicSize = Mathf.Clamp(m_Camera.orthographicSize, ZoomSizeMin, ZoomSizeMax);
    }

    float ComputeZoomSpeedModifier()
    {
        float zoomRatio = Mathf.Clamp(1f - (ZoomSizeMax - transform.position.y) / (ZoomSizeMax - ZoomSizeMin), 0f, 1f);
        float zoomSpeedModifier = MoveSpeedFromZoomCurve.Evaluate(zoomRatio);
        //Debug.Log("zoomSpeedModifier " + zoomSpeedModifier);

        return zoomSpeedModifier;

        //float zoomRatio = Mathf.Clamp(1f - (ZoomSizeMax - m_Camera.orthographicSize) / (ZoomSizeMax - ZoomSizeMin), 0f, 1f);
        // float zoomSpeedModifier = MoveSpeedFromZoomCurve.Evaluate(zoomRatio);
        //Debug.Log("zoomSpeedModifier " + zoomSpeedModifier);

        //return zoomSpeedModifier;
    }
    public void MouseMove(Vector2 move)
    {
        if (Mathf.Approximately(move.sqrMagnitude, 0f))
            return;

        MoveHorizontal(move.x);
        MoveVertical(move.y);
    }
    public void KeyboardMoveHorizontal(float value)
    {
        MoveHorizontal(value * KeyboardSpeedModifier);
    }
    public void KeyboardMoveVertical(float value)
    {
        MoveVertical(value * KeyboardSpeedModifier);
    }
    public void MoveHorizontal(float value)
    {
        Move.x += value * MoveSpeed * ComputeZoomSpeedModifier() * Time.deltaTime;
    }
    public void MoveVertical(float value)
    {
        Move.z += value * MoveSpeed * ComputeZoomSpeedModifier() * Time.deltaTime;
    }

    // Direct focus on one entity (no smooth)
    public void FocusEntity(BaseEntity entity)
    {
        if (entity == null)
            return;

        Vector3 newPos = entity.transform.position;
        newPos.y = transform.position.y;

        transform.position = newPos;
    }

	#endregion

	#region MonoBehaviour methods
	private void Awake()
	{
        m_Camera = GetComponent<Camera>();
	}
	void Start()
    {
        TerrainSize = GameServices.GetTerrainSize();
    }
    void Update()
    {
        if (Move != Vector3.zero)
        {
            transform.position += Move;
            if (EnableMoveLimits)
            {
                // Clamp camera position (max height, terrain bounds)
                Vector3 newPos = transform.position;
                newPos.x = Mathf.Clamp(transform.position.x, TerrainBorder, TerrainSize.x - TerrainBorder);
                newPos.z = Mathf.Clamp(transform.position.z, TerrainBorder, TerrainSize.z - TerrainBorder);
                transform.position = newPos;
            }
        }

        Move = Vector3.zero;
    }
    #endregion
}
