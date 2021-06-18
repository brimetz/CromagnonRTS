public class SequenceLoop : Composite
{
    int m_CurrentChild = 0;

    public SequenceLoop(string compositeName, params Node[] nodes) : base(compositeName, nodes)
    {
    }

    public override NodeStatus OnBehave(BehaviourState state)
    {
        NodeStatus ret = m_Children[m_CurrentChild].Behave(state);

        switch (ret)
        {
            case NodeStatus.Success:
                m_CurrentChild++;
                break;

            case NodeStatus.Failure:
                m_CurrentChild++;
                break;
        }

        if (m_CurrentChild >= m_Children.Count)
        {
            return NodeStatus.Success;
        }
        else if (ret == NodeStatus.Success)
        {
            // if we succeeded, don't wait for the next tick to process the next child
            return OnBehave(state);
        }

        return NodeStatus.Running;
    }

    public override void OnReset()
    {
        m_CurrentChild = 0;
        foreach (Node child in m_Children)
        {
            child.Reset();
        }
    }
}
