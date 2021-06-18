using System;
using UnityEngine;

public class FogOfWarTexture : MonoBehaviour
{
    private Texture2D m_Texture;

    [SerializeField]
    private Color m_GreyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    [SerializeField]
    protected Color m_WhiteColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField]
    private Color m_StartColor = new Color(0, 0, 0, 1.0f);

    [SerializeField]
    private SpriteRenderer m_SpriteRenderer;

    [SerializeField]
    private float m_InterpolateSpeed;

    [NonSerialized]
    private Color[] m_Colors;

    public void CreateTexture(int width, int height, Vector2 scale)
    {
        m_Texture = new Texture2D(width, height);
        m_SpriteRenderer.sprite = Sprite.Create(m_Texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1);
        m_SpriteRenderer.transform.localScale = scale;

        int size = width * height;
        m_Colors = new Color[size];
        for (int i = 0; i < size; ++i)
            m_Colors[i] = m_StartColor;

        m_Texture.SetPixels(m_Colors);
        m_Texture.Apply();
    }

    public void SetTexture(Grid visibleGrid, Grid previousVisibleGrid, int team)
    {
        for (int i = 0; i < visibleGrid.Size; ++i)
        {
            bool isVisible = (visibleGrid.Get(i) & team) == team;
            bool wasVisible = (previousVisibleGrid.Get(i) & team) == team;

            Color newColor = m_StartColor;
            if (isVisible)
                newColor = m_WhiteColor;
            else if (wasVisible)
                newColor = m_GreyColor;

            newColor.r = Mathf.Lerp(m_Colors[i].r, newColor.r, Time.deltaTime * m_InterpolateSpeed);
            m_Colors[i] = newColor;
        }

        m_Texture.SetPixels(m_Colors);
        m_Texture.Apply();
    }
}
