using Solver.Framework;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SolverTests")]

namespace Solver.Components;

public class Tile : IResolvableComponent, ILinkable, IFreezable
{
    private bool _frozen = false;

    public Tile(
        Coordinates coordinates,
        Shape shape,
        Resolution resolution = Resolution.Unknown)
    {
        ArgumentNullException.ThrowIfNull(shape);

        Coordinates = coordinates;
        Orientation = shape.Orientation;
        Vertices = [.. shape.VertexOffsets.Select(offset => coordinates + offset)];
        Resolution = resolution;
    }

    public Tile(
        Coordinates coordinates,
        Orientation orientation,
        IEnumerable<Coordinates> vertices,
        Resolution resolution = Resolution.Unknown)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        Coordinates = coordinates;
        Orientation = orientation;
        Vertices = [.. vertices];
        Resolution = resolution;
    }

    public bool IsFrozen => _frozen;

    public void Freeze()
    {
        _frozen = true;
    }

    /// <summary>
    /// Coordinates of the centre of the tile
    /// </summary>
    public Coordinates Coordinates { get; }

    /// <summary>
    /// Orientation of the tile
    /// </summary>
    /// <remarks>
    /// A tile with orientation Up is an upward-pointing equilateral triangle.
    /// A tile with orientation Down is an downward-pointing equilateral triangle.
    /// </remarks>
    public Orientation Orientation { get; }

    /// <summary>
    /// Indicates whether the tile has an edge that is a border of the grid
    /// </summary>
    public bool HasBorder { get; private set; }

    /// <summary>
    /// Always returns false
    /// </summary>
    public bool IsTerminal => false;

    /// <summary>
    /// List of the 3 coordinates that specify the vertices of the tile 
    /// </summary>
    public IReadOnlyList<Coordinates> Vertices { get; }

    /// <summary>
    /// Reference to the aisle containing the tile that is normal to the X-axis 
    /// </summary>
    public Aisle AisleX { get; private set; }

    /// <summary>
    /// Reference to the aisle containing the tile that is normal to the Y-axis 
    /// </summary>
    public Aisle AisleY { get; private set; }

    /// <summary>
    /// Reference to the aisle containing the tile that is normal to the Z-axis 
    /// </summary>
    public Aisle AisleZ { get; private set; }

    /// <summary>
    /// Returns the aisle containing the tile that is normal to the specified axis
    /// </summary>
    public Aisle GetAisle(Axis axis)
    {
        return axis switch
        {
            Axis.X => AisleX,
            Axis.Y => AisleY,
            Axis.Z => AisleZ,
            _ => throw new UnreachableException($"Usupported axis: {axis}")
        };
    }

    /// <summary>
    /// Enumeration of the aisles containing this tile, ordered by the aisle normal axes (X, Y, Z) 
    /// </summary>
    public IEnumerable<Aisle> Aisles
    {
        get
        {
            yield return AisleX;
            yield return AisleY;
            yield return AisleZ;
        }
    }

    /// <summary>
    /// Reference to the tile edge that is normal to the X-axis 
    /// </summary>
    /// <remarks>
    /// The lower edge for a tile with an Up orientation.
    /// The upper edge for a tile with a Down orientation.
    /// </remarks>
    public Edge EdgeX { get; private set; }

    /// <summary>
    /// Reference to the tile edge that is normal to the Y-axis 
    /// </summary>
    /// <remarks>
    /// The top-right edge for a tile with an Up orientation.
    /// The bottom-left edge for a tile with a Down orientation.
    /// </remarks>
    public Edge EdgeY { get; private set; }

    /// <summary>
    /// Reference to the tile edge that is normal to the Z-axis 
    /// </summary>
    /// <remarks>
    /// The top-left edge for a tile with an Up orientation.
    /// The bottom-right edge for a tile with a Down orientation.
    /// </remarks>
    public Edge EdgeZ { get; private set; }

    /// <summary>
    /// List of the 3 edges that form the boundary of the tile, ordered by the edge normal axes (X, Y, Z)
    /// </summary>
    public IEnumerable<Edge> Edges
    {
        get
        {
            yield return EdgeX;
            yield return EdgeY;
            yield return EdgeZ;
        }
    }

    /// <summary>
    /// Returns the edge of this tile that is normal to the specified axis
    /// </summary>
    public Edge GetEdge(Axis axis)
    {
        return axis switch
        {
            Axis.X => EdgeX,
            Axis.Y => EdgeY,
            Axis.Z => EdgeZ,
            _ => throw new UnreachableException($"Usupported axis: {axis}")
        };
    }

    internal void SetAisles(IEnumerable<Aisle> aisles)
    {
        if (_frozen)
        {
            throw new InvalidOperationException("Object instance is frozen.");
        }

        ArgumentNullException.ThrowIfNull(aisles);

        if (!SolverGrid.IncludesAllAxes(aisles.Select(aisle => aisle.Axis)))
        {
            throw new ArgumentException("Must include an aisle for all axes.", nameof(aisles));
        }

        foreach (var aisle in aisles)
        {
            switch (aisle.Axis)
            {
                case Axis.X:
                    AisleX = aisle;
                    break;

                case Axis.Y:
                    AisleY = aisle;
                    break;

                case Axis.Z:
                    AisleZ = aisle;
                    break;
            }
        }
    }

    internal void SetEdges(IEnumerable<Edge> edges)
    {
        ArgumentNullException.ThrowIfNull(edges);

        if (_frozen)
        {
            throw new InvalidOperationException("Object instance is frozen.");
        }

        if (!SolverGrid.IncludesAllAxes(edges.Select(edge => edge.NormalAxis)))
        {
            throw new ArgumentException("Must include edges normal to each axis.", nameof(edges));
        }

        foreach (var edge in edges)
        {
            switch (edge.NormalAxis)
            {
                case Axis.X:
                    EdgeX = edge;
                    break;

                case Axis.Y:
                    EdgeY = edge;
                    break;

                case Axis.Z:
                    EdgeZ = edge;
                    break;
            }

            if (edge.IsBorder)
            {
                HasBorder = true;
            }
        }
    }

    /// <summary>
    /// Returns the resolution state of the tile
    /// </summary>
    public Resolution Resolution { get; private set; }

    /// <summary>
    /// Indicates whether the tile is resolved or not
    /// </summary>
    public bool IsResolved => Resolution != Resolution.Unknown;

    /// <summary>
    /// Attempts to set the resolution state of this tile, returning true if the state was changed
    /// </summary>
    /// <remarks>
    /// Invokes the notifier if the state changes from Unknown to a resolved state (Empty or Channel).
    /// </remarks>
    public bool TryResolve(
        Resolution resolution,
        INotifier notifier,
        ResolutionReason reason = ResolutionReason.Unspecified)
    {
        if (Resolution != Resolution.Unknown || resolution == Resolution.Unknown)
        {
            return false;
        }

        Resolution = resolution;
        notifier.NotifyResolution(this, reason);
        return true;
    }

    public override int GetHashCode()
    {
        return Coordinates.GetHashCode();
    }

    public override string ToString()
    {
        return $"Tile:{Orientation}:{Coordinates}=>{Resolution}";
    }
}
