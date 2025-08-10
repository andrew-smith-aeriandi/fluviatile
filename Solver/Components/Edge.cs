using Solver.Framework;
using Solver.Rules;

namespace Solver.Components;

public class Edge : IResolvableComponent, IFreezable
{
    private bool _frozen;

    public Edge(
        Coordinates v1,
        Coordinates v2,
        SolverGrid grid)
    {
        ArgumentNullException.ThrowIfNull(grid);

        if (v1 == v2)
        {
            throw new ArgumentException($"Coordinates must differ: [{v1}, {v2}].", $"{nameof(v1)},{nameof(v2)}");
        }

        if (v1.X == v2.X)
        {
            NormalAxis = Axis.X;
            IsBorder = Math.Abs(v1.X) == grid.Radius;
        }
        else if (v1.Y == v2.Y)
        {
            NormalAxis = Axis.Y;
            IsBorder = Math.Abs(v1.Y) == grid.Radius;
        }
        else if (v1.Z == v2.Z)
        {
            NormalAxis = Axis.Z;
            IsBorder = Math.Abs(v1.Z) == grid.Radius;
        }
        else
        {
            throw new ArgumentException($"Unable to determine normal axis from coordinate pair.");
        }

        Vertices = new(v1, v2);
        Resolution = Resolution.Unknown;
    }

    public bool IsFrozen => _frozen;

    public void Freeze()
    {
        _frozen = true;
    }

    public UnorderedPair<Coordinates> Vertices { get; }

    public Axis NormalAxis { get; }

    public bool IsBorder { get; }

    public bool? IsExit => (IsBorder, Resolution) switch
    {
        (true, Resolution.Channel) => true,
        (true, Resolution.Empty) => false,
        (true, _) => null,
        (false, _) => false
    };

    public Tile? TilePlus { get; private set; }

    public Tile? TileMinus { get; private set; }

    public IEnumerable<Tile> Tiles
    {
        get
        {
            if (TileMinus is not null)
            {
                yield return TileMinus;
            }

            if (TilePlus is not null)
            {
                yield return TilePlus;
            }
        }
    }

    internal void SetTiles(Tile? tile1, Tile? tile2)
    {
        if (_frozen)
        {
            throw new InvalidOperationException("Object instance is frozen.");
        }

        (TileMinus, TilePlus) =
            tile1?.Orientation == Orientation.Down ||
            tile2?.Orientation == Orientation.Up
                ? (tile2, tile1)
                : (tile1, tile2);
    }

    public Resolution Resolution { get; private set; }

    public bool IsResolved => Resolution != Resolution.Unknown;

    public bool TryResolve(Resolution resolution, INotifier notifier, ResolutionReason reason = ResolutionReason.Unspecified)
    {
        if (resolution == Resolution.Unknown || Resolution != Resolution.Unknown)
        {
            return false;
        }

        Resolution = resolution;
        notifier.NotifyResolution(this, reason);
        return true;
    }

    public override int GetHashCode()
    {
        return Vertices.GetHashCode();
    }

    public override string ToString()
    {
        return $"{(IsBorder ? "Border" : "Edge")}:{Vertices}=>{Resolution}";
    }
}
