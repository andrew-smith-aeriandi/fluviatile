using System.Diagnostics.CodeAnalysis;

namespace Solver.Components;

public class TileComparer : IEqualityComparer<Tile>
{
    public readonly static TileComparer Default = new();

    public bool Equals(Tile? x, Tile? y)
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

    public int GetHashCode([DisallowNull] Tile obj)
    {
        return obj.Coordinates.GetHashCode();
    }
}
