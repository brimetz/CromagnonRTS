using UnityEngine;

public class Inverter : Decorator
{
    public Inverter(Node child) : base(child)
    {
    }

    public override NodeStatus OnBehave(BehaviourState state)
    {
        switch (m_Child.Behave(state))
        {
            case NodeStatus.Running:
                return NodeStatus.Running;

            case NodeStatus.Success:
                return NodeStatus.Failure;

            case NodeStatus.Failure:
                return NodeStatus.Success;
        }

        Debug.LogError("Inverter::OnBehave - Should not get here");
        return NodeStatus.Failure;
    }

    public override void OnReset()
    {
    }
}
