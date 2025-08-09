namespace Solver.Framework;

public class VertexState<T>
{
    public VertexState(T value)
    {
        Value = value;
        Adjacency = [];
        IsVisited = false;
        IsArticulationPoint = false;
        Discovered = 0;
        Low = 0;
    }

    public T Value { get; }

    public List<VertexState<T>> Adjacency { get; }

    public bool IsVisited { get; private set; }

    public bool IsArticulationPoint { get; private set; }

    public int Discovered { get; private set; }

    public int Low { get; set; }

    public void SetVisited(int time)
    {
        IsVisited = true;
        Discovered = time;
        Low = time;
    }

    public void SetArticulationPoint()
    {
        IsArticulationPoint = true;
    }
}
