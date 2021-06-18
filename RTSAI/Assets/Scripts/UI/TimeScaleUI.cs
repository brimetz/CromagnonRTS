using UnityEngine;
using UnityEngine.UI;

public class TimeScaleUI : MonoBehaviour
{
    [SerializeField]
    private Button m_x1Button;
    [SerializeField]
    private Button m_x2Button;

    [SerializeField]
    private float m_TimeScaleOne;
    [SerializeField]
    private float m_TimeScaleTwo;

    [HideInInspector]
    public float TimeScale;
    private void Awake()
    {
        m_x1Button.onClick.AddListener(TimeScaleSetToOne);
        m_x2Button.onClick.AddListener(TimeScaleSetToTwo);
    }

	private void Start()
	{
        TimeScaleSetToOne();
    }

    private void TimeScaleSetToOne()
	{
        m_x1Button.interactable = false;
        m_x2Button.interactable = true;
        Time.timeScale = m_TimeScaleOne;
        TimeScale = m_TimeScaleOne;
    }

    private void TimeScaleSetToTwo()
    {
        m_x2Button.interactable = false;
        m_x1Button.interactable = true;
        Time.timeScale = m_TimeScaleTwo;
        TimeScale = m_TimeScaleTwo;
    }
}
