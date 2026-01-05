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

    public Axis Axis { get; }

    public int Index { get; }

    public bool IsMargin { get; }

    public IReadOnlyList<Edge> Borders { get; private set; }

    public IReadOnlyList<Tile> Tiles { get; private set; }

    public int TileCount { get; }

    public int ResolvedTileCount { get; private set; }

    public int UnresolvedTileCount => TileCount - ResolvedTileCount;

    public int ChannelTileCount { get; }

    public int ResolvedChannelTileCount { get; private set; }

    public int UnresolvedChannelTileCount => ChannelTileCount - ResolvedChannelTileCount;

    public int EmptyTileCount { get; }

    public int ResolvedEmptyTileCount { get; private set; }

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
