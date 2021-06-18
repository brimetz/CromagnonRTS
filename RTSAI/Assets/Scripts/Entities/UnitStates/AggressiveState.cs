using System;
using System.Linq;
using UnityEngine;

public class AggressiveState : IState
{
	private Unit m_Unit;

	public AggressiveState(Unit unit)
	{
		m_Unit = unit;
	}

	public void OnEnter()
	{
		Debug.Log("AggressiveState::OnEnter");
		// Stop Repair
		if (m_Unit.EntityTarget != null && m_Unit.EntityTarget.GetTeam() == m_Unit.GetTeam())
			m_Unit.EntityTarget = null;
	}

	public void OnExit()
	{
		Debug.Log("AggressiveState::OnExit");
	}

	public void OnUpdate()
	{
		Debug.Log("AggressiveState::OnUpdate");

		if (m_Unit.OnTask != null && m_Unit.CanTask())
		{
			m_Unit.NavMeshAgent.Finish();
			m_Unit.OnTask();
			m_Unit.OnTask = null;
		}
		else
		{
			Unit[] targets = GameServices.GetControllerByTeam(m_Unit.GetTeam().GetEnemyTeam())
				.UnitList.Where(t => m_Unit.CanAttack(t)).ToArray();

			Array.Sort(targets, delegate (Unit unit1, Unit unit2)
			{
				return Vector3.Distance(m_Unit.transform.position, unit1.transform.position).CompareTo(Vector3.Distance(m_Unit.transform.position, unit2.transform.position));
			});

			Unit target = targets.FirstOrDefault();

			if (target != null)
			{
				m_Unit.StopCapture();
				m_Unit.StartAttacking(target);
			}
			else
			{
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
