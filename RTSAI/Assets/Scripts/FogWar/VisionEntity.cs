using UnityEngine;

public class VisionEntity : MonoBehaviour
{
    public ETeam Team;
    public float Range;
    public Vector2 Position => new Vector2(transform.position.x, transform.position.z);

    private bool m_IsVisibleDefault = true;
    private bool m_IsVisibleUI = true;

    [SerializeField]
    private GameObject[] GameObjectDefaultLayer;
    [SerializeField]
    private GameObject[] GameObjectUILayer;
    [SerializeField]
    private GameObject[] GameObjectMinimapLayer;

    private bool m_HasSeen;
    public bool HasSeen
	{
        get { return m_HasSeen; }
        set { if (m_HasSeen == value) return; else m_HasSeen = value; }
	}

    public void SetVisible(bool visible)
	{
        SetVisibleDefault(visible);
        SetVisibleUI(visible);
    }

    public void SetVisibleUI(bool visible)
	{
        if (m_IsVisibleUI == visible)
            return;

        if (visible)
		{
            m_IsVisibleUI = true;
            SetLayer(GameObjectUILayer, LayerMask.NameToLayer("UI"));
        }
        else
        {
            m_IsVisibleUI = false;
            SetLayer(GameObjectUILayer, LayerMask.NameToLayer("Hidden"));
        }
    }

    public void SetVisibleDefault(bool visible)
    {
        if (m_IsVisibleDefault == visible)
            return;

        if (visible)
        {
            m_IsVisibleDefault = true;
            SetLayer(GameObjectDefaultLayer, LayerMask.NameToLayer("Default"));
            SetLayer(GameObjectMinimapLayer, LayerMask.NameToLayer("Minimap"));
        }
        else
        {
            m_IsVisibleDefault = false;
            SetLayer(GameObjectDefaultLayer, LayerMask.NameToLayer("Hidden"));
            SetLayer(GameObjectMinimapLayer, LayerMask.NameToLayer("Hidden"));
        }

    }

    void SetLayer(GameObject[] gameObjects, int newLayer)
    {
        foreach (GameObject obj in gameObjects)
            obj.layer = newLayer;
    }
}
