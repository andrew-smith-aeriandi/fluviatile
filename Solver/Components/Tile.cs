using Solver.Framework;
using Solver.Rules;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SolverTests")]

namespace Solver.Components;

public class Tile : IResolvableComponent, ILinkable, IFreezable
{
    private bool _frozen = false;

    public Tile(
        Coordinates coordinates,
        Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);

        Coordinates = coordinates;
        Orientation = shape.Orientation;
        Vertices = [.. shape.VertexOffsets.Select(offset => coordinates + offset)];
    }

    public bool IsFrozen => _frozen;

    public void Freeze()
    {
        _frozen = true;
    }

    public Coordinates Coordinates { get; }

    public Orientation Orientation { get; }

    public bool HasBorder { get; private set; }

    public bool IsTerminal => false;

    public IReadOnlyList<Coordinates> Vertices { get; }

    public Aisle AisleX { get; private set; }

    public Aisle AisleY { get; private set; }

    public Aisle AisleZ { get; private set; }

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

    public IEnumerable<Aisle> Aisles
    {
        get
        {
            yield return AisleX;
            yield return AisleY;
            yield return AisleZ;
        }
    }

    public Edge EdgeX { get; private set; }

    public Edge EdgeY { get; private set; }

    public Edge EdgeZ { get; private set; }

    public IEnumerable<Edge> Edges
    {
        get
        {
            yield return EdgeX;
            yield return EdgeY;
            yield return EdgeZ;
        }
    }

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

    public Resolution Resolution { get; private set; }

    public bool IsResolved => Resolution != Resolution.Unknown;

    public bool TryResolve(Resolution resolution, INotifier notifier, ResolutionReason reason = ResolutionReason.Unspecified)
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
