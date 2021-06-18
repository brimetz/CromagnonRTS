public abstract class Task
{
    protected bool m_IsFinish = false;

    public bool IsFinish
	{
        get { return m_IsFinish; }
        set { m_IsFinish = value; }
	}
}
