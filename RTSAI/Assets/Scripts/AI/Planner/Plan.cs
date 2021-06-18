public class Plan
{
    // Position in the list of Current Task
    protected int m_IdCurrentTask = 0;

    // If true, We just change the current task, so we need to execute it
    protected bool m_LaunchTask = true;

    // If the plan is finish
    public bool IsFinish = false;

    public int CurrentTask
	{
		set { m_IdCurrentTask = value; }
	}
}
