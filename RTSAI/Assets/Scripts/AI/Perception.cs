using UnityEngine;

public class Perception : MonoBehaviour
{
    public BehaviourTree BehaviourTree;
    public EGoal Goal => BehaviourTree.GoalPriority.Goal;

    private void Start()
    {
        BehaviourTree = new BehaviourTree();
        BehaviourTree.Init(transform.GetComponent<AIController>());
    }

    void FixedUpdate()
    {
        if (BehaviourTree != null)
            BehaviourTree.FixedUpdateBT();
    }

    public void Reset()
    {
        BehaviourTree.Reset();
    }
}
