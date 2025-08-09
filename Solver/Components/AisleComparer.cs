using System.Diagnostics.CodeAnalysis;

namespace Solver.Components;

public class AisleComparer : IEqualityComparer<Aisle>
{
    public readonly static AisleComparer Default = new();

    public bool Equals(Aisle? x, Aisle? y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        if (ReferenceEquals(x, y))
        {
            return true;
        }

        return x.Axis == y.Axis && x.Index == y.Index;
    }

    public int GetHashCode([DisallowNull] Aisle obj)
    {
        return (int)obj.Axis << 8 | obj.Index;
    }
}
