using Solver.Framework;

namespace Solver.Components;

public class Edge : IResolvableComponent, IFreezable
{
    private bool _frozen;

    public Edge(
        Coordinates v1,
        Coordinates v2,
        SolverGrid grid,
        Resolution resolution = Resolution.Unknown)
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
            Vertices = v1.Y > v2.Y ? new(v2, v1) : new(v1, v2);
        }
        else if (v1.Y == v2.Y)
        {
            NormalAxis = Axis.Y;
            IsBorder = Math.Abs(v1.Y) == grid.Radius;
            Vertices = v1.Z > v2.Z ? new(v2, v1) : new(v1, v2);
        }
        else if (v1.Z == v2.Z)
        {
            NormalAxis = Axis.Z;
            IsBorder = Math.Abs(v1.Z) == grid.Radius;
            Vertices = v1.X > v2.X ? new(v2, v1) : new(v1, v2);
        }
        else
        {
            throw new ArgumentException($"Unable to determine normal axis from coordinate pair.");
        }

        Resolution = resolution;
    }

    public bool IsFrozen => _frozen;

    public void Freeze()
    {
        _frozen = true;
    }

    /// <summary>
    /// Unordered coordinate pair representing the two verices of the edge
    /// </summary>
    public UnorderedPair<Coordinates> Vertices { get; }

    public Axis NormalAxis { get; }

    /// <summary>
    /// Indicates whether the edge is a border of the grid
    /// </summary>
    public bool IsBorder { get; }

    /// <summary>
    /// Indicates whether the edge is an exit
    /// </summary>
    /// <remarks>
    /// Returns false for all edges that are not borders, since exits must be on the border of the grid. 
    /// Returns null if the edge is an unresolved border.
    /// Returns false if the edge is an empty border.
    /// Returns true if the edge is a border with a channel.
    /// </remarks>
    public bool? IsExit => (IsBorder, Resolution) switch
    {
        (true, Resolution.Channel) => true,
        (true, Resolution.Empty) => false,
        (true, _) => null,
        (false, _) => false
    };

    /// <summary>
    /// Reference to the tile adjacent to the edge in the positive direction normal to the edge  
    /// </summary>
    /// <remarks>
    /// Returns null if the edge is a border with no tile in the positive normal direction
    /// </remarks>
    public Tile? TilePlus { get; private set; }

    /// <summary>
    /// Reference to the tile adjacent to the edge in the negative direction normal to the edge  
    /// </summary>
    /// <remarks>
    /// Returns null if the edge is a border with no tile in the negative normal direction
    /// </remarks>
    public Tile? TileMinus { get; private set; }

    /// <summary>
    /// Returns an enumeration of the tiles that are adjacent to the edge
    /// </summary>
    /// <remarks>
    /// The enumeration will include either one or two tiles.
    /// If the enumeration includes two tiles, the first will be the one adjacent to the edge
    /// in the negative normal direction.
    /// </remarks>
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

    /// <summary>
    /// Returns the resolution state of the edge
    /// </summary>
    public Resolution Resolution { get; private set; }

    /// <summary>
    /// Indicates whether the edge is resolved or not
    /// </summary>
    public bool IsResolved => Resolution != Resolution.Unknown;

    /// <summary>
    /// Attempts to set the resolution state of this edge, returning true if the state was changed
    /// </summary>
    /// <remarks>
    /// Invokes the notifier if the state changes from Unknown to a resolved state (Empty or Channel).
    /// </remarks>
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
