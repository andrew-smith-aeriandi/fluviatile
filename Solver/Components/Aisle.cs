using Solver.Framework;
using System.Diagnostics;

namespace Solver.Components;

public class Aisle : IComponent, IFreezable
{
    private bool _frozen = false;

    public Aisle(
        Axis axis,
        int index,
        bool isMargin,
        int tileCount,
        int channelCount,
        int resolvedChannelCount = 0,
        int resolvedEmptyCount = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(tileCount);
        ArgumentOutOfRangeException.ThrowIfNegative(channelCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(channelCount, tileCount);

        Axis = axis;
        Index = index;
        IsMargin = isMargin;
        TileCount = tileCount;
        ChannelTileCount = channelCount;
        EmptyTileCount = tileCount - channelCount;

        ResolvedChannelTileCount = resolvedChannelCount;
        ResolvedEmptyTileCount = resolvedEmptyCount;
        ResolvedTileCount = ResolvedChannelTileCount + ResolvedEmptyTileCount;
    }

    public bool IsFrozen => _frozen;

    public void Freeze()
    {
        _frozen = true;
    }

    /// <summary>
    /// Aisle axis: X, Y or Z
    /// </summary>
    public Axis Axis { get; }

    /// <summary>
    /// Aisle index: zero-based
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Indicates whether the aisle is on the margin of the grid
    /// </summary>
    public bool IsMargin { get; }

    /// <summary>
    /// List of all the edges of tiles in the aisle that are borders of the grid
    /// </summary>
    public IReadOnlyList<Edge> Borders { get; private set; }

    /// <summary>
    /// List of all the tiles in the aisle
    /// </summary>
    public IReadOnlyList<Tile> Tiles { get; private set; }

    /// <summary>
    /// Number of tiles in aisle
    /// </summary>
    public int TileCount { get; }

    /// <summary>
    /// Number of resolved tiles in aisle 
    /// </summary>
    public int ResolvedTileCount { get; private set; }

    /// <summary>
    /// Number of unresolved tiles in aisle 
    /// </summary>
    public int UnresolvedTileCount => TileCount - ResolvedTileCount;

    /// <summary>
    /// Number of channel tiles in aisle regardless of their current resolution 
    /// </summary>
    public int ChannelTileCount { get; }

    /// <summary>
    /// Number of resolved channel tiles in aisle 
    /// </summary>
    public int ResolvedChannelTileCount { get; private set; }

    /// <summary>
    /// Number of unresolved channel tiles in aisle 
    /// </summary>
    /// <remarks>
    /// If this value is zero, all other unresolved tiles must be empty
    /// </remarks>
    public int UnresolvedChannelTileCount => ChannelTileCount - ResolvedChannelTileCount;

    /// <summary>
    /// Number of empty tiles in aisle regardless of their current resolution 
    /// </summary>
    public int EmptyTileCount { get; }

    /// <summary>
    /// Number of resolved empty tiles in aisle 
    /// </summary>
    public int ResolvedEmptyTileCount { get; private set; }

    /// <summary>
    /// Number of unresolved empty tiles in aisle 
    /// </summary>
    /// <remarks>
    /// If this value is zero, all other unresolved tiles must be channels
    /// </remarks>
    public int UnresolvedEmptyTileCount => EmptyTileCount - ResolvedEmptyTileCount;

    public void NotifyResolution(Tile tile)
    {
        switch (tile.Resolution)
        {
            case Resolution.Channel:
                if (UnresolvedChannelTileCount <= 0)
                {
                    throw new UnreachableException($"{nameof(UnresolvedChannelTileCount)} cannot be negative");
                }

                if (UnresolvedTileCount <= 0)
                {
                    throw new UnreachableException($"{nameof(UnresolvedTileCount)} cannot be negative");
                }

                ResolvedChannelTileCount += 1;
                ResolvedTileCount += 1;
                break;

            case Resolution.Empty:
                if (UnresolvedEmptyTileCount <= 0)
                {
                    throw new UnreachableException($"{nameof(UnresolvedEmptyTileCount)} cannot be negative");
                }

                if (UnresolvedTileCount <= 0)
                {
                    throw new UnreachableException($"{nameof(UnresolvedTileCount)} cannot be negative");
                }

                ResolvedEmptyTileCount += 1;
                ResolvedTileCount += 1;
                break;
        }
    }

    public void SetTiles(IEnumerable<Tile> tiles)
    {
        if (_frozen)
        {
            throw new InvalidOperationException("Object instance is frozen.");
        }

        var aisleTiles = tiles.OrderBy(this.SortOrderKeySelector()).ToArray();
        if (aisleTiles.Length != TileCount)
        {
            throw new ArgumentException($"Collection has {aisleTiles.Length} entries but {TileCount} were expected.", nameof(tiles));
        }

        var borders = aisleTiles
            .SelectMany(tile => tile.Edges)
            .Where(edge => edge.IsBorder)
            .ToArray();

        Tiles = aisleTiles;
        Borders = borders;
    }

    public override int GetHashCode()
    {
        return (int)Axis << 8 | Index;
    }

    public override string ToString()
    {
        return $"Aisle:{Axis}[{Index}]=>{ChannelTileCount}/{TileCount}";
    }
}
