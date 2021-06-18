public class Succeeder : Decorator
{
    public Succeeder(Node child) : base(child)
    {
    }

    public override NodeStatus OnBehave(BehaviourState state)
    {
        NodeStatus ret = m_Child.Behave(state);

        if (ret == NodeStatus.Running)
            return NodeStatus.Running;

        return NodeStatus.Success;
    }

    public override void OnReset()
    {
    }
}
