using UnityEngine;

public class FogOfWarSystem : MonoBehaviour
{
    public int GridWidth;
    public int GridHeight;

    [SerializeField]
    protected FogOfWarTexture m_FogOfWarTexture;
    [SerializeField]
    protected Camera m_FogCamera;
    [SerializeField]
    protected Transform m_FogQuadParent;

    private Grid m_VisibleGrid;
    private Grid m_PreviousVisibleGrid;
    private Vector2 m_TextureScale;

    public void Init()
    {
        m_TextureScale = new Vector2(m_FogQuadParent.localScale.x / GridWidth,
                                    m_FogQuadParent.localScale.y / GridHeight);
        m_FogOfWarTexture.CreateTexture(GridWidth, GridHeight, m_TextureScale);

        m_VisibleGrid = new Grid(GridWidth, GridHeight, 0);
        m_PreviousVisibleGrid = new Grid(GridWidth, GridHeight, 0);
    }

    private Vector2Int GetPositionInGrid(Vector2 p)
    {
        return new Vector2Int
        {
            x = Mathf.RoundToInt(p.x * GridWidth / m_FogQuadParent.localScale.x),
            y = Mathf.RoundToInt(p.y * GridHeight / m_FogQuadParent.localScale.y)
        };
    }

    private void SetCell(int team, int x, int y)
    {
        if (!m_VisibleGrid.Contains(x, y) && (m_VisibleGrid.Values[x + y * m_VisibleGrid.Height] & team) > 0)
            return;

        m_VisibleGrid.Values[x + y * m_VisibleGrid.Width] |= team;
        m_PreviousVisibleGrid.Values[x + y * m_PreviousVisibleGrid.Width] |= team;
    }

    public void ClearVision()
    {
        m_VisibleGrid.Clear();
    }

    public void UpdateVisions(VisionEntity[] visions)
    {
        foreach (VisionEntity vision in visions)
        {
            Vector2Int gridPos = GetPositionInGrid(vision.Position);
            
            int radius = Mathf.FloorToInt(vision.Range / m_TextureScale.x) - 1;
            if (radius <= 0)
                return;

            int x = radius;
            int y = 0;
            int xTemp = 1 - (radius * 2);
            int yTemp = 0;
            int radiusTemp = 0;

            while (x >= y)
            {
                for (int j = gridPos.x - x; j <= gridPos.x + x; ++j)
                {
                    SetCell(1 << (int)vision.Team, j, gridPos.y + y);
                    SetCell(1 << (int)vision.Team, j, gridPos.y - y);
                }
                for (int j = gridPos.x - y; j <= gridPos.x + y; ++j)
                {
                    SetCell(1 << (int)vision.Team, j, gridPos.y + x);
                    SetCell(1 << (int)vision.Team, j, gridPos.y - x);
                }

                ++y;
                radiusTemp += yTemp;
                yTemp += 2;

                if (((radiusTemp * 2) + xTemp) > 0)
                {
                    x--;
                    radiusTemp += xTemp;
                    xTemp += 2;
                }
            }
        }
    }

    public void UpdateTextures(int team)
    {
        m_FogOfWarTexture.SetTexture(m_VisibleGrid, m_PreviousVisibleGrid, team);
    }

    public bool IsVisible(int team, Vector2 position)
    {
        Vector2Int posGrid = GetPositionInGrid(position);
        return m_VisibleGrid.IsValue(team, posGrid.x, posGrid.y);
    }

    public bool WasVisible(int team, Vector2 position)
    {
        Vector2Int posGrid = GetPositionInGrid(position);
        return m_PreviousVisibleGrid.IsValue(team, posGrid.x, posGrid.y);
    }
}
