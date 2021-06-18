public class Selector : Composite
{
    private int m_CurrentChild = 0;

    public Selector(string compositeName, params Node[] nodes) : base(compositeName, nodes)
    {
    }
    
    public override NodeStatus OnBehave(BehaviourState state)
    {
        if (m_CurrentChild >= m_Children.Count)
        {
            return NodeStatus.Failure;
        }

        NodeStatus nodeStatus = m_Children[m_CurrentChild].Behave(state);

        switch (nodeStatus)
        {
            case NodeStatus.Success:
                return NodeStatus.Success;

            case NodeStatus.Failure:
                m_CurrentChild++;

            // If we failed, immediately process the next child
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
