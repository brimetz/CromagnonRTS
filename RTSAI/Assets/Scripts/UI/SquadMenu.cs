using System;
using UnityEngine;
using UnityEngine.UI;

public class SquadMenu : MonoBehaviour
{
	[SerializeField]
	private Button[] m_FormationButtons;
	[SerializeField]
	private Button[] m_PostureButtons;

	[SerializeField]
	private Color m_SelectColor = new Color(0.20f, 0.60f, 0.85f);
	[SerializeField]
	private Color m_UnselectColor = new Color(0.20f, 0.28f, 0.37f);

	public void UpdateFormationUI(Squad squad)
	{
		foreach (EFormation formation in Enum.GetValues(typeof(EFormation)) as EFormation[])
			 ChangeColorButtonFormation(squad, formation);
	}

	public void UpdatePostureUI(Squad squad)
	{
		foreach (EPosture posture in Enum.GetValues(typeof(EPosture)) as EPosture[])
		{
			ChangeColorButtonPosture(squad, posture);
			if (posture == EPosture.Repair)
				m_PostureButtons[(int)posture].gameObject.SetActive(squad.CanRepair());
		}
	}

	public void ChangeColorButtonFormation(Squad squad, EFormation formation)
	{
		int index = (int)formation;

		if (squad.currentFormation == formation)
			m_FormationButtons[index].targetGraphic.color = m_SelectColor;
		else
			m_FormationButtons[index].targetGraphic.color = m_UnselectColor;
	}

	public void ChangeColorButtonPosture(Squad squad, EPosture posture)
	{
		int index = (int)posture;

		if ((squad.GetPosture() & (1 << index)) == 1 << index)
			m_PostureButtons[index].targetGraphic.color = m_SelectColor;
		else
			m_PostureButtons[index].targetGraphic.color = m_UnselectColor;
	}
}
