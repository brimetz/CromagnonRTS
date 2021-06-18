using UnityEngine;

public class PassiveState : IState
{
	private Unit m_Unit;

	public PassiveState(Unit unit)
	{
		m_Unit = unit;
	}

	public void OnEnter()
	{
		Debug.Log("PassiveState::OnEnter");
		// Stop Attack
		if (m_Unit.EntityTarget != null && m_Unit.EntityTarget.GetTeam() != m_Unit.GetTeam())
		{
			m_Unit.EntityTarget = null;
			m_Unit.SetTargetPos(m_Unit.TargetPos, m_Unit.DistanceTask);
		}
	}

	public void OnExit()
	{
		Debug.Log("PassiveState::OnExit");
	}

	public void OnUpdate()
	{
		Debug.Log("PassiveState::OnUpdate");

		if (m_Unit.OnTask != null && m_Unit.CanTask())
		{
			m_Unit.NavMeshAgent.Finish();
			m_Unit.OnTask();
			m_Unit.OnTask = null;
		}
	}
}
