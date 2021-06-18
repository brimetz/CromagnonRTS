using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_Menu;
    [SerializeField]
    private TimeScaleUI m_TimeScaleUI;

    private bool m_Display = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
		{
            m_Display = !m_Display;
            if (m_Display)
            {
                Time.timeScale = 0f;
                m_Menu.SetActive(true);
            }
            else
			{
                Resume();
            }
        }
    }

    public void Resume()
	{
        m_Menu.SetActive(false);
        Time.timeScale = m_TimeScaleUI.TimeScale;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = m_TimeScaleUI.TimeScale;
    }

    public void Quit()
	{
        Application.Quit();
	}
}
