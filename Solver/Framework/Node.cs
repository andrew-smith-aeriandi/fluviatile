namespace Solver.Framework;

public class Node<T>
{
    private readonly List<Node<T>> _children;

    public Node(T value)
    {
        Value = value;
        _children = [];
    }

    public T Value { get; }

    public IEnumerable<Node<T>> Children => _children;

    public void AddChild(Node<T> child)
    {
        _children.Add(child);
    }

    public void AddChildren(params IEnumerable<Node<T>> children)
    {
        _children.AddRange(children);
    }
}
