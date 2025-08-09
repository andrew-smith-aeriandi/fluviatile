using System.Diagnostics.CodeAnalysis;

namespace Solver.Components;

public class LocationComparer : IEqualityComparer<ILocatable>
{
    public readonly static LocationComparer Default = new();

    public bool Equals(ILocatable? x, ILocatable? y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        if (ReferenceEquals(x, y))
        {
            return true;
        }

        return x.Coordinates == y.Coordinates;
    }

    public int GetHashCode([DisallowNull] ILocatable obj)
    {
        return obj.GetHashCode();
    }
}
