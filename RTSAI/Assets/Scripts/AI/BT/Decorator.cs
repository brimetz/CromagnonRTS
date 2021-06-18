public abstract class Decorator : Node
{
    protected Node m_Child;

    public Decorator(Node node)
    {
        m_Child = node;
    }
}
