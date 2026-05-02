using Solver.Framework;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Solver.Components;

public partial class Thalweg : IComponent
{
    private readonly SolverGrid _grid;
    private readonly int _channelTileCount;
    private int _linkedChannelTileCount;

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
        _channelTileCount = tileCount;
        _linkedChannelTileCount = 0;

        var linkCount = tileCount + SolverGrid.TerminationCount;
        _membership = new(linkCount, LocationComparer.Default);
        _segments = new(linkCount / 2);

        _terminations = new List<Termination>(SolverGrid.TerminationCount);
    }

    public bool TryAddTermination(Termination termination)
    {
        ArgumentNullException.ThrowIfNull(termination);

        if (_terminations.Contains(termination, LocationComparer.Default))
        {
            // Termination already exists
            return false;
        }

        _terminations.Add(termination);
        return true;
    }

    public void CreateSegment(params IEnumerable<ILinkable> links)
    {
        var segment = new Segment(this, links);
        _linkedChannelTileCount += segment.ChannelTileCount;
    }

    /// <summary>
    /// References the associated grid
    /// </summary>
    public SolverGrid Grid => _grid;

    /// <summary>
    /// List of resolved segments
    /// </summary>
    /// <remarks>
    /// For a solved tableau, there is a single continuous channel, so this list will have a single element.
    /// </remarks>
    public IReadOnlyList<Segment> Segments => _segments;

    /// <summary>
    /// Count of resolved segments
    /// </summary>
    /// <remarks>
    /// Returns 1 for a solved tableau since there is a single continuous channel.
    /// </remarks>
    public int SegmentCount => _segments.Count;

    /// <summary>
    /// Count of tiles containing channels in the grid
    /// </summary>
    public int ChannelTileCount => _channelTileCount;

    /// <summary>
    /// Count of tiles that are part of a resolved thalweg segment in the tableau
    /// </summary>
    /// <remarks>
    /// For a solved tableau, this property will have the same value as the ChannelTileCount property.
    /// </remarks>
    public int LinkedChannelTileCount => _linkedChannelTileCount;

    /// <summary>
    /// Count of tiles that are not part of a resolved thalweg segment in the tableau
    /// </summary>
    /// <remarks>
    /// Returns 0 for a solved tableau.
    /// </remarks>
    public int UnlinkedChannelTileCount => _channelTileCount - _linkedChannelTileCount;

    public IReadOnlyList<Termination> Terminations => _terminations;

    /// <summary>
    /// Always returns 2
    /// </summary>
    public int TerminationCount => SolverGrid.TerminationCount;

    /// <summary>
    /// Returns the number of resolved terminations, either 0, 1 or 2.
    /// </summary>
    /// <remarks>
    /// Returns 2 for a solved tableau.
    /// </remarks>
    public int ResolvedTerminationCount => _terminations.Count;

    /// <summary>
    /// Returns the number of unresolved terminations, either 0, 1 or 2.
    /// </summary>
    /// <remarks>
    /// Returns 0 for a solved tableau.
    /// </remarks>
    public int UnresolvedTerminationCount => SolverGrid.TerminationCount - _terminations.Count;

    /// <summary>
    /// Attempts to retrieve the resolved thalweg segment that contains the specified linkable component (Tile or Termination)
    /// </summary>
    public bool TryGetSegment(ILinkable link, [MaybeNullWhen(false)] out Segment segment)
    {
        if (link is not null && _membership.TryGetValue(link, out segment))
        {
            return true;
        }

        segment = null;
        return false;
    }

    public bool TryGetTermination(
        Coordinates coordinates,
        [MaybeNullWhen(false)] out Termination termination)
    {
        termination = _terminations.Find(t => t.Coordinates.Equals(coordinates));
        return termination is not null;
    }

    /// <summary>
    /// Indicates whether the specified component is included in a resolved thalweg segment
    /// </summary>
    public bool IsLinked(ILinkable component)
    {
        return _membership.ContainsKey(component);
    }

    /// <summary>
    /// Attempts to join linkable components into a resolved thalweg segment
    /// </summary>
    public bool TryLink(
        Edge edge,
        INotifier notifier,
        ResolutionReason reason = ResolutionReason.Unspecified)
    {
        ArgumentNullException.ThrowIfNull(edge);

        if (edge.TileMinus is null && edge.TilePlus is null)
        {
            throw new InvalidOperationException($"{nameof(edge)}.{nameof(edge.TileMinus)} and {nameof(edge)}.{nameof(edge.TilePlus)} cannot both be null.");
        }

        if (edge.Tiles.Any(t => t.Resolution != Resolution.Channel))
        {
            throw new ArgumentException($"Any adjacent tiles must be resolved as channels: {edge}", nameof(edge));
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
                _linkedChannelTileCount += 1;
                isLinked = true;
            }
            else if (tile == segment.Last)
            {
                segment.AddToLast(otherTile);
                _linkedChannelTileCount += 1;
                isLinked = true;
            }
        }
        else if (otherSegment is not null && tile is not null && otherTile is not null)
        {
            // Attempt to add a node to a channel segment
            if (otherTile == otherSegment.First)
            {
                otherSegment.AddToFirst(tile);
                _linkedChannelTileCount += 1;
                isLinked = true;
            }
            else if (otherTile == otherSegment.Last)
            {
                otherSegment.AddToLast(tile);
                _linkedChannelTileCount += 1;
                isLinked = true;
            }
        }
        else if (tile is not null && otherTile is not null)
        {
            // Create a new channel segment linking the two tiles
            CreateSegment(tile, otherTile);
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
                    if (_terminations.Count >= SolverGrid.TerminationCount)
                    {
                        throw new InvalidOperationException($"Number of exits cannot exceed {SolverGrid.TerminationCount}");
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
                    if (_terminations.Count >= SolverGrid.TerminationCount)
                    {
                        throw new InvalidOperationException($"Number of exits cannot exceed {SolverGrid.TerminationCount}");
                    }

                    CreateSegment(tile, termination);
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
