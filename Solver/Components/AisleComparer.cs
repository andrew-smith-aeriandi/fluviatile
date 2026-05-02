using Solver.Framework;
using System.Diagnostics.CodeAnalysis;

namespace Solver.Components;

public class AisleComparer : IEqualityComparer<Aisle>, IComparer<Aisle>
{
    public readonly static AisleComparer Default = new();

    /// <summary>
    /// Order by Axis (X, Y, Z) then by Index
    /// </summary>
    public int Compare(Aisle? x, Aisle? y)
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

        return (x.Axis, y.Axis) switch
        {
            (Axis.X, Axis.X) or (Axis.Y, Axis.Y) or (Axis.Z, Axis.Z) => x.Index - y.Index,
            (Axis.Y, Axis.X) or (Axis.Z, Axis.X) or (Axis.Z, Axis.Y) => 1,
            _ => -1,
        };
    }

    public bool Equals(Aisle? x, Aisle? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.Axis == y.Axis && x.Index == y.Index;
    }

    public int GetHashCode([DisallowNull] Aisle obj)
    {
        return (int)obj.Axis << 8 | obj.Index;
    }
}
