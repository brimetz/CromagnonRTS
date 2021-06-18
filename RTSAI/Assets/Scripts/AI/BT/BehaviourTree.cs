using UnityEngine;

public class BehaviourTree
{
    private Node m_BehaviourTree;
    private Context m_BehaviourState;
    private EGoal m_ActualGoal = EGoal.None;
    public GoalPriority GoalPriority = new GoalPriority();

    public void Init(AIController AI)
    {
        m_BehaviourTree = CreateBehaviourTree();
        m_BehaviourState = new Context(AI);
    }

    public void FixedUpdateBT()
    {
        if (m_BehaviourTree != null)
        {
            m_BehaviourTree.Behave(m_BehaviourState);
            Context context = m_BehaviourState;
            if (context.EndGoal.Goal != EGoal.None)
                GoalPriority.Goal = context.EndGoal.Goal;
            if (GoalPriority.Goal != EGoal.None && m_ActualGoal != GoalPriority.Goal)
            {
                Debug.Log("BehaviourTree::Goal - " + GoalPriority.Goal.ToString());
                m_ActualGoal = GoalPriority.Goal;
            }
        }
    }

    Node CreateBehaviourTree()
    {
        SequenceLoop goal = new SequenceLoop("Goal",
                                     new Conquer(),
                                     new ConstructLightUT(),
                                     new ConstructHeavyUT(),
                                     new Defend(),
                                     new Destroy(),
                                     new Explore(),
                                     new Secure(),
                                     new Reparation());

        return new Repeater(goal);
    }

    public void Reset()
    {
        Context context = m_BehaviourState;
        context.EndGoal.Goal = EGoal.None;
        context.EndGoal.Priority = EPriority.None;
        m_ActualGoal = GoalPriority.Goal = EGoal.None;
        GoalPriority.Priority = EPriority.None;
    }
}
