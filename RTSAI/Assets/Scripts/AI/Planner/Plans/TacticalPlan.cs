using System.Collections.Generic;

public class TacticalPlan : Plan
{
    // List of action in the plan
    private List<TacticalTask> m_Tasks = new List<TacticalTask>();

    private Squad m_OwnSquad = null;
    private bool m_NeedRepeat = false;

    public int TaskCount => m_Tasks.Count;

    public void SetSquad(Squad squad)
    {
        m_OwnSquad = squad;
    }

    public void SetNeedRepeat(bool repeat)
    {
        m_NeedRepeat = repeat;

        foreach (TacticalTask Task in m_Tasks)
            Task.IsFinish = false;
    }

    public void Execute()
    {
        if (m_OwnSquad == null)
            return;

        if (m_Tasks.Count == 0)
            IsFinish = true;

        if (!IsFinish)
        {
            TacticalTask CurrentTask = m_Tasks[m_IdCurrentTask];

            // first update of new task, we need to launch it
            if (m_LaunchTask)
            {
                m_NeedRepeat = false;
                CurrentTask.Execute(m_OwnSquad);

                // Need to repeat the launch of task, if a task is repeated
                if (!m_NeedRepeat)
                    m_LaunchTask = false;
            }

            if (!m_NeedRepeat)
                CurrentTask.Update(m_OwnSquad);

            if (CurrentTask.IsFinish)
            {
                m_LaunchTask = true;
                m_IdCurrentTask++;
                if (m_IdCurrentTask == m_Tasks.Count)
                    IsFinish = true;
            }
        }
    }

    public void AddForwardTask(TacticalTask task)
    {
        m_Tasks.Insert(0, task);
    }

    public TacticalTask GetCurrentTask()
    {
        if (m_Tasks.Count > m_IdCurrentTask)
            return m_Tasks[m_IdCurrentTask];

        return null;
    }
}
