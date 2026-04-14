using System.Diagnostics.CodeAnalysis;

namespace Solver.Framework;

public class CoordinatesComparer : IEqualityComparer<Coordinates>, IComparer<Coordinates>
{
    public readonly static CoordinatesComparer XAxis = new(Axis.X);
    public readonly static CoordinatesComparer YAxis = new(Axis.Y);
    public readonly static CoordinatesComparer ZAxis = new(Axis.Z);
    public readonly static CoordinatesComparer Default = XAxis;

    public CoordinatesComparer(Axis primaryAxis)
    {
        PrimaryAxis = primaryAxis;
    }

    public Axis PrimaryAxis { get; }

    public int Compare(Coordinates x, Coordinates y)
    {
        return PrimaryAxis switch
        {
            Axis.Z => x.Z == y.Z ? x.X - y.X : x.Z - y.Z,
            Axis.Y => x.Y == y.Y ? x.Z - y.Z : x.Y - y.Y,
            _ => x.X == y.X ? x.Y - y.Y : x.X - y.X,
        };
    }

    public bool Equals(Coordinates x, Coordinates y)
    {
        return x.Equals(y);
    }

    public int GetHashCode([DisallowNull] Coordinates obj)
    {
        return obj.GetHashCode();
    }
}
