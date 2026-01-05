using Solver.Framework;
using System.Diagnostics.CodeAnalysis;

namespace Solver.Components;

public class EdgeComparer : IEqualityComparer<Edge>, IComparer<Edge>
{
    public readonly static EdgeComparer Default = new();

    public int Compare(Edge? x, Edge? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }
        else if (x is null)
        {
            return 1;
        }
        else if (y is null)
        {
            return -1;
        }

        return (x.NormalAxis, y.NormalAxis) switch
        {
            (Axis.X, Axis.X) => CoordinatesComparer.XAxis.Compare(x.Vertices[0], y.Vertices[0]),
            (Axis.Y, Axis.Y) => CoordinatesComparer.YAxis.Compare(x.Vertices[0], y.Vertices[0]),
            (Axis.Z, Axis.Z) => CoordinatesComparer.ZAxis.Compare(x.Vertices[0], y.Vertices[0]),
            (Axis.Y, Axis.X) or (Axis.Z, Axis.Y) or (Axis.Z, Axis.X) => 1,
            _ => -1,
        };
    }

    public bool Equals(Edge? x, Edge? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.Vertices == y.Vertices;
    }

    public int GetHashCode([DisallowNull] Edge obj)
    {
        return obj.GetHashCode();
    }
}
