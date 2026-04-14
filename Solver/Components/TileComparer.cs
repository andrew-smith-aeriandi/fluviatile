using Solver.Framework;
using System.Diagnostics.CodeAnalysis;

namespace Solver.Components;

public class TileComparer : IEqualityComparer<Tile>, IComparer<Tile>
{
    public readonly static TileComparer Default = new();

    private readonly CoordinatesComparer _coordinatesComparer = CoordinatesComparer.Default;

    public int Compare(Tile? x, Tile? y)
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

        return _coordinatesComparer.Compare(x.Coordinates, y.Coordinates);
    }

    public bool Equals(Tile? x, Tile? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return _coordinatesComparer.Equals(x.Coordinates, y.Coordinates);
    }

    public int GetHashCode([DisallowNull] Tile obj)
    {
        return obj.Coordinates.GetHashCode();
    }
}
