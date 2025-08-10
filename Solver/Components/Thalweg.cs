using Solver.Framework;
using Solver.Rules;
using System.Diagnostics;

namespace Solver.Components;

public partial class Thalweg : IComponent
{
    private readonly SolverGrid _grid;
    private readonly int _tileCount;
    private int _linkedTileCount;

    private readonly Dictionary<ILinkable, Segment> _membership;
    private readonly List<Termination> _terminations;
    private readonly List<Segment> _segments;

    public Thalweg(
        SolverGrid grid,
        int tileCount)
    {
        ArgumentNullException.ThrowIfNull(grid);

        if (!tileCount.IsInRange(0, grid.TileCount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(tileCount),
                $"Value must be in the range [0, {grid.TileCount}].");
        }

        _grid = grid;
        _tileCount = tileCount;
        _linkedTileCount = 0;

        var linkCount = tileCount + SolverGrid.ExitCount;
        _membership = new(linkCount, LocationComparer.Default);
        _segments = new(linkCount / 2);
        _terminations = new List<Termination>(SolverGrid.ExitCount);
    }

    public IReadOnlyList<Segment> Segments => _segments;

    public int SegmentCount => _segments.Count;

    public int TileCount => _tileCount;

    public int LinkedTileCount => _linkedTileCount;

    public int UnlinkedTileCount => _tileCount - _linkedTileCount;

    public IReadOnlyList<Termination> Exits => _terminations;

    public int ExitCount => SolverGrid.ExitCount;

    public int ResolvedExitCount => _terminations.Count;

    public int UnresolvedExitCount => SolverGrid.ExitCount - _terminations.Count;

    public bool TryGetSegment(ILinkable link, out Segment? segment)
    {
        if (link is not null && _membership.TryGetValue(link, out segment))
        {
            return true;
        }

        segment = null;
        return false;
    }

    public bool IsLinked(ILinkable component)
    {
        return _membership.ContainsKey(component);
    }

    public bool TryLink(Edge edge, INotifier notifier, ResolutionReason reason = ResolutionReason.Unspecified)
    {
        ArgumentNullException.ThrowIfNull(edge);

        if (edge.TileMinus is null && edge.TilePlus is null)
        {
            throw new InvalidOperationException($"{nameof(edge)}.{nameof(edge.TileMinus)} and {nameof(edge)}.{nameof(edge.TilePlus)} cannot both be null.");
        }

        if (edge.Tiles.Any(t => t.Resolution != Resolution.Channel))
        {
            return false;
            //TODO: Fix this
            //throw new ArgumentException($"Any adjacent tiles must be resolved as channels: {edge}", nameof(edge));
        }

        if (edge.Resolution != Resolution.Channel)
        {
            throw new ArgumentException($"Edge must be resolved as a channel: {edge}", nameof(edge));
        }

        var isLinked = false;

        var tile = edge.TileMinus;
        var segment = tile is not null && _membership.TryGetValue(tile, out var segmentMinus)
            ? segmentMinus
            : null;

        var otherTile = edge.TilePlus;
        var otherSegment = otherTile is not null && _membership.TryGetValue(otherTile, out var segmentPlus)
            ? segmentPlus
            : null;

        if (segment is not null && otherSegment is not null)
        {
            if (ReferenceEquals(segment, otherSegment))
            {
                // Already linked
                return false;
            }

            if (segment.Count < otherSegment.Count)
            {
                // Swap references so segment is the longer, thus reducing the number of copy operations
                (segment, otherSegment) = (otherSegment, segment);
                (tile, otherTile) = (otherTile, tile);
            }

            // Attempt to link two channel segments
            if (tile == segment.First && otherTile == otherSegment.First)
            {
                segment.AddFirstToFirst(otherSegment);
                isLinked = true;
            }
            else if (tile == segment.First && otherTile == otherSegment.Last)
            {
                segment.AddLastToFirst(otherSegment);
                isLinked = true;
            }
            else if (tile == segment.Last && otherTile == otherSegment.First)
            {
                segment.AddFirstToLast(otherSegment);
                isLinked = true;
            }
            else if (tile == segment.Last && otherTile == otherSegment.Last)
            {
                segment.AddLastToLast(otherSegment);
                isLinked = true;
            }
        }
        else if (segment is not null && tile is not null && otherTile is not null)
        {
            // Attempt to add a node to a channel segment
            if (tile == segment.First)
            {
                segment.AddToFirst(otherTile);
                _linkedTileCount += 1;
                isLinked = true;
            }
            else if (tile == segment.Last)
            {
                segment.AddToLast(otherTile);
                _linkedTileCount += 1;
                isLinked = true;
            }
        }
        else if (otherSegment is not null && tile is not null && otherTile is not null)
        {
            // Attempt to add a node to a channel segment
            if (otherTile == otherSegment.First)
            {
                otherSegment.AddToFirst(tile);
                _linkedTileCount += 1;
                isLinked = true;
            }
            else if (otherTile == otherSegment.Last)
            {
                otherSegment.AddToLast(tile);
                _linkedTileCount += 1;
                isLinked = true;
            }
        }
        else if (tile is not null && otherTile is not null)
        {
            // Create a new channel segment linking the two tiles
            var newSegment = new Segment(this, tile, otherTile);
            _linkedTileCount += newSegment.TileCount;
            isLinked = true;
        }
        else if (edge.IsBorder && (segment is not null || otherSegment is not null))
        {
            // Attempt to terminate channel segment
            segment ??= otherSegment ?? throw new UnreachableException("Both segments cannot be null.");
            tile ??= otherTile ?? throw new UnreachableException("Both tiles cannot be null.");

            if (SolverGrid.TryGetAdjacentCoordinates(tile, edge.NormalAxis, out var coordinates))
            {
                var termination = new Termination(coordinates, edge);
                if (!_membership.ContainsKey(termination))
                {
                    if (_terminations.Count >= SolverGrid.ExitCount)
                    {
                        throw new InvalidOperationException($"Number of exits cannot exceed {SolverGrid.ExitCount}");
                    }

                    if (tile == segment.First)
                    {
                        segment.AddToFirst(termination);
                        notifier.NotifyResolution(termination, reason);
                        isLinked = true;
                    }
                    else if (tile == segment.Last)
                    {
                        segment.AddToLast(termination);
                        notifier.NotifyResolution(termination, reason);
                        isLinked = true;
                    }
                }
            }
        }
        else if (edge.IsBorder && (tile is not null || otherTile is not null))
        {
            // Create a new channel segment linking tile to termination
            tile ??= otherTile ?? throw new UnreachableException("Both tiles cannot be null.");

            if (SolverGrid.TryGetAdjacentCoordinates(tile, edge.NormalAxis, out var coordinates))
            {
                var termination = new Termination(coordinates, edge);
                if (!_membership.ContainsKey(termination))
                {
                    if (_terminations.Count >= SolverGrid.ExitCount)
                    {
                        throw new InvalidOperationException($"Number of exits cannot exceed {SolverGrid.ExitCount}");
                    }

                    var newSegment = new Segment(this, tile, termination);
                    _linkedTileCount += newSegment.TileCount;

                    notifier.NotifyResolution(termination, reason);
                    isLinked = true;
                }
            }
        }

        return isLinked;
    }

    public override string ToString()
    {
        return string.Join(Environment.NewLine, Segments);
    }
}
