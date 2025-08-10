using System.Diagnostics.CodeAnalysis;

namespace Solver.Components;

public class EdgeComparer : IEqualityComparer<Edge>
{
    public readonly static EdgeComparer Default = new();

    public bool Equals(Edge? x, Edge? y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        if (ReferenceEquals(x, y))
        {
            return true;
        }

        return x.Vertices == y.Vertices;
    }

    public int GetHashCode([DisallowNull] Edge obj)
    {
        return obj.GetHashCode();
    }
}
