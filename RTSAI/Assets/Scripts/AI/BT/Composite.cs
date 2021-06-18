using System.Collections.Generic;
using UnityEngine;

public abstract class Composite : Node
{
    protected List<Node> m_Children = new List<Node>();
    public string CompositeName;

    public Composite(string name, params Node[] nodes)
    {
        CompositeName = name;
        m_Children.AddRange(nodes);
    }

    public override NodeStatus Behave(BehaviourState state)
    {
        if (m_Debug && Ticks == 0)
            Debug.Log("Composite::Behave - Running behaviour list: " + CompositeName);

        NodeStatus node = base.Behave(state);
        if (m_Debug && node != NodeStatus.Running)
            Debug.Log("Composite::Behave - Behaviour list " + CompositeName + " returned: " + node.ToString());

        return node;
    }
}
