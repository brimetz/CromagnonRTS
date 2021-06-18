using System.Collections.Generic;

public class StrategicalPlan : Plan
{
    // List of action in the plan
    private List<StrategicalTask> m_Tasks = new List<StrategicalTask>();

    private AIController m_OwnAI = null;

    public void Execute()
    {
        // If Task.Count == 0, The plan is not achieve during the construction, we need to delete it
        if (m_Tasks.Count == 0)
            IsFinish = true;

        if (!IsFinish)
        {
            StrategicalTask CurrentTask = m_Tasks[m_IdCurrentTask];

            // first update of new task, we need to launch it
            if (m_LaunchTask)
            {
                CurrentTask.Execute(m_OwnAI);
                m_LaunchTask = false;
            }

            CurrentTask.Update(m_OwnAI);

            if (CurrentTask.IsFinish)
            {
                m_LaunchTask = true;
                m_IdCurrentTask++;
                if (m_IdCurrentTask == m_Tasks.Count)
                    IsFinish = true;
            }
        }
    }

    public void SetAI(AIController AI)
    {
        m_OwnAI = AI;
    }

    public void AddTask(StrategicalTask task)
    {
        m_Tasks.Add(task);
    }
}
