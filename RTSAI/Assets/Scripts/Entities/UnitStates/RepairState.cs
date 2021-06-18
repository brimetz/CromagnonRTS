using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RepairState : IState
{
	private Unit m_Unit;

	public RepairState(Unit unit)
	{
		m_Unit = unit;
	}

	public void OnEnter()
	{
		if (!m_Unit.GetUnitData.CanRepair)
			Debug.LogError("RepairState::OnEnter - Unit can't be in RepairState!");

		// Stop Attack
		if (m_Unit.EntityTarget != null && m_Unit.EntityTarget.GetTeam() != m_Unit.GetTeam())
		m_Unit.EntityTarget = null;
	}

	public void OnExit()
	{
		Debug.Log("RepairState::OnExit");

		// StopRepair
		if (m_Unit.EntityTarget != null && m_Unit.EntityTarget.GetTeam() == m_Unit.GetTeam())
			m_Unit.EntityTarget = null;
	}

	public void OnUpdate()
	{
		Debug.Log("RepairState::OnUpdate");

		if (m_Unit.OnTask != null && m_Unit.CanTask())
		{
			m_Unit.NavMeshAgent.Finish();
			m_Unit.OnTask();
			m_Unit.OnTask = null;
		}
		else
		{
			List<Unit> units = GameServices.GetControllerByTeam(m_Unit.GetTeam())
				.UnitList.Where(t => m_Unit.CanRepair(t) && t.GetHP() < t.GetUnitData.MaxHP).ToList();

			List<Factory> factories = GameServices.GetControllerByTeam(m_Unit.GetTeam())
				.FactoryList.Where(t => m_Unit.CanRepair(t) && t.GetHP() < t.GetFactoryData.MaxHP).ToList();

			List<BaseEntity> baseEntities = new List<BaseEntity>();
			baseEntities.AddRange(units);
			baseEntities.AddRange(factories);

			foreach (BaseEntity a in baseEntities)
				Debug.Log(a.name);

			BaseEntity[] targets = baseEntities.ToArray();

			Array.Sort(targets, delegate (BaseEntity unit1, BaseEntity unit2)
			{
				return Vector3.Distance(m_Unit.transform.position, unit1.transform.position).CompareTo(Vector3.Distance(m_Unit.transform.position, unit2.transform.position));
			});

			BaseEntity target = targets.FirstOrDefault();

			if (target != null)
			{
				m_Unit.StopCapture();
				m_Unit.StartRepairing(target);
			}
			else
			{
				m_Unit.EntityTarget = null;
				if (m_Unit.CaptureTargetTemp != null)
					m_Unit.StartCapture(m_Unit.CaptureTargetTemp);
				else if (m_Unit.AttackFactoryTemp != null)
					m_Unit.StartAttacking(m_Unit.AttackFactoryTemp);

				if (m_Unit.NavMeshAgent.isStopped)
					m_Unit.NavMeshAgent.isStopped = false;
			}
		}
	}
}
