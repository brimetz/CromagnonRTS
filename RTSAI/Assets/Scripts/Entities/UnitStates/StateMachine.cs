public class StateMachine
{
    private IState m_CurrentState;

    public void SetState(IState state)
    {
        m_CurrentState?.OnExit();
        m_CurrentState = state;
        m_CurrentState.OnEnter();
    }

    public void Update()
    {
        m_CurrentState?.OnUpdate();
    }
}
