public class Repeater : Decorator
{
    public Repeater(Node child) : base(child)
    {
    }

    public override NodeStatus OnBehave(BehaviourState state)
    {
        NodeStatus ret = m_Child.Behave(state);
        if (ret != NodeStatus.Running)
        {
            Reset();
            m_Child.Reset();
        }
        return NodeStatus.Success;
    }

    public override void OnReset()
    {
    }
}
