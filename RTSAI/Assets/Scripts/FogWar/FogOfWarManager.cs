using System;
using System.Collections;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
	public ETeam FogOfWarTeam;

	private ETeam PlayerTeam => FogOfWarTeam;
	private ETeam AITeam => FogOfWarTeam.GetEnemyTeam();

	[SerializeField]
    private FogOfWarSystem m_VisionSystem;

	public FogOfWarSystem VisionSystem
	{
		get { return m_VisionSystem; }
	}

	[SerializeField]
	private float UpdateEveryImagePerSeconds = 30f;

	void Start()
    {
		m_VisionSystem.Init();
        StartCoroutine(UpdateVision());
    }

	private IEnumerator UpdateVision()
	{
		while (true)
		{
			UpdateVisionTextures();
			UpdateUnitVisibility();
			UpdateBuildingVisibility();
			UpdateFactoriesVisibility();

			yield return new WaitForSeconds((1f / UpdateEveryImagePerSeconds) * Time.timeScale);
		}
	}

	private void UpdateVisionTextures()
	{
		m_VisionSystem.ClearVision();
		m_VisionSystem.UpdateVisions(FindObjectsOfType<VisionEntity>());
		m_VisionSystem.UpdateTextures(1 << (int)PlayerTeam);
	}

	private void UpdateUnitVisibility()
	{
		foreach (Unit unit in GameServices.GetControllerByTeam(PlayerTeam).UnitList)
		{
			unit.Vision.SetVisible(true);
			if (m_VisionSystem.IsVisible(1 << (int)AITeam, unit.Vision.Position))
				GameServices.GetControllerByTeam(AITeam).AddUnitSeen(unit);
		}

		foreach (Unit unit in GameServices.GetControllerByTeam(AITeam).UnitList)
		{
			if (m_VisionSystem.IsVisible(1 << (int)PlayerTeam, unit.Vision.Position))
			{
				GameServices.GetControllerByTeam(PlayerTeam).AddUnitSeen(unit);
				unit.Vision.SetVisible(true);
			}
			else
				unit.Vision.SetVisible(false);
		}
	}

	private void UpdateBuildingVisibility()
	{
		foreach (TargetBuilding building in GameServices.GetTargetBuildings())
		{
			if (m_VisionSystem.IsVisible(1 << (int)PlayerTeam, building.Vision.Position))
			{
				building.UpdateTeam(PlayerTeam);
				building.UpdateTeamMaterial(PlayerTeam);
				building.Vision.SetVisibleUI(true);
				GameServices.GetControllerByTeam(PlayerTeam).AddTargetBuildingsSeen(building);
			}
			else
			{
				building.UpdateTeamMaterial(PlayerTeam);
				building.Vision.SetVisibleUI(false);
			}

			if (m_VisionSystem.IsVisible(1 << (int)AITeam, building.Vision.Position))
				GameServices.GetControllerByTeam(AITeam).AddTargetBuildingsSeen(building);

			if (m_VisionSystem.WasVisible(1 << (int)PlayerTeam, building.Vision.Position))
				building.Vision.SetVisibleDefault(true);
			else
				building.Vision.SetVisible(false);
		}
	}

	private void UpdateFactoriesVisibility()
	{
		foreach (Factory factory in GameServices.GetControllerByTeam(PlayerTeam).FactoryList)
		{
			factory.Vision.SetVisible(true);
			if (m_VisionSystem.IsVisible(1 << (int)AITeam, factory.Vision.Position))
				GameServices.GetControllerByTeam(AITeam).AddFactorySeen(factory);
		}

		foreach (Factory factory in GameServices.GetControllerByTeam(AITeam).FactoryList)
		{
			if (m_VisionSystem.IsVisible(1 << (int)PlayerTeam, factory.Vision.Position))
			{
				factory.Vision.SetVisibleUI(true);
				GameServices.GetControllerByTeam(PlayerTeam).AddFactorySeen(factory);
			}
			else
				factory.Vision.SetVisibleUI(false);

			if (m_VisionSystem.WasVisible(1 << (int)PlayerTeam, factory.Vision.Position))
				factory.Vision.SetVisibleDefault(true);
			else
				factory.Vision.SetVisible(false);
		}
	}
}
