using Solver.Framework;
using System.Diagnostics;

namespace Solver.Components;

public class Aisle : IComponent, IFreezable
{
    private bool _frozen = false;

    public Aisle(
        Axis axis,
        int index,
        bool isBorder,
        int tileCount,
        int channelCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(tileCount);
        ArgumentOutOfRangeException.ThrowIfNegative(channelCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(channelCount, tileCount);

        Axis = axis;
        Index = index;
        IsMargin = isBorder;
        TileCount = tileCount;
        ChannelTileCount = channelCount;
        EmptyTileCount = tileCount - channelCount;

        UnresolvedTileCount = TileCount;
        UnresolvedChannelTileCount = ChannelTileCount;
        UnresolvedEmptyTileCount = EmptyTileCount;
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

    public int UnresolvedTileCount { get; private set; }

    public int ResolvedTileCount => TileCount - UnresolvedTileCount;

    public int ChannelTileCount { get; }

    public int UnresolvedChannelTileCount { get; private set; }

    public int ResolvedChannelTileCount => ChannelTileCount - UnresolvedChannelTileCount;

    public int EmptyTileCount { get; }

    public int UnresolvedEmptyTileCount { get; private set; }

    public int ResolvedEmptyTileCount => EmptyTileCount - UnresolvedEmptyTileCount;

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

                UnresolvedChannelTileCount -= 1;
                UnresolvedTileCount -= 1;
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

                UnresolvedEmptyTileCount -= 1;
                UnresolvedTileCount -= 1;
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
