using UnityEngine;

public class DestroyOnDelay : MonoBehaviour
{
    [SerializeField]
    private float m_SecondBeforeDestroy = 1f;

    private float m_CurrentTimeSinceSpawn = 0f;

    void Update()
    {
        m_CurrentTimeSinceSpawn += Time.deltaTime;
        if (m_SecondBeforeDestroy <= m_CurrentTimeSinceSpawn)
            Destroy(gameObject);
    }
}
