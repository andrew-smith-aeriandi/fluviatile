using Solver.Framework;
using System.Collections.Frozen;

namespace Solver.Components;

public class Tableau : IComponent
{
    public Tableau(
        SolverGrid grid,
        Thalweg thalweg,
        IEnumerable<Aisle> aisles,
        IEnumerable<Tile> tiles,
        IEnumerable<Edge> edges)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(thalweg);
        ArgumentNullException.ThrowIfNull(aisles);
        ArgumentNullException.ThrowIfNull(tiles);
        ArgumentNullException.ThrowIfNull(edges);

        Grid = grid;
        Thalweg = thalweg;
        Aisles = aisles.ToFrozenDictionary(aisle => aisle.GetDefaultKey());
        Tiles = tiles.ToFrozenDictionary(tile => tile.GetDefaultKey());
        Edges = edges.ToFrozenDictionary(edge => edge.GetDefaultKey());

        ChannelCount = aisles.Where(aisle => aisle.Axis == Axis.X).Sum(aisle => aisle.ChannelTileCount);
        EmptyCount = TileCount - ChannelCount;
        UnresolvedTileCount = TileCount;
        UnresolvedChannelCount = ChannelCount;
        UnresolvedEmptyCount = EmptyCount;
    }

    public int TileCount => Grid.TileCount;

    public int UnresolvedTileCount { get; private set; }

    public int ResolvedTileCount => Grid.TileCount - UnresolvedTileCount;

    public int ChannelCount { get; }

    public int UnresolvedChannelCount { get; private set; }

    public int ResolvedChannelCount => ChannelCount - UnresolvedChannelCount;

    public int EmptyCount { get; }

    public int UnresolvedEmptyCount { get; private set; }

    public int ResolvedEmptyCount => EmptyCount - UnresolvedEmptyCount;

    public SolverGrid Grid { get; }

    public Thalweg Thalweg { get; }

    public FrozenDictionary<(Axis, int), Aisle> Aisles { get; }

    public FrozenDictionary<Coordinates, Tile> Tiles { get; }

    public FrozenDictionary<UnorderedPair<Coordinates>, Edge> Edges { get; }

    public void NotifyResolution(IComponent component)
    {
        switch (component)
        {
            case Tile tile:
                foreach (var aisle in tile.Aisles)
                {
                    aisle.NotifyResolution(tile);
                }

                switch (tile.Resolution)
                {
                    case Resolution.Channel:
                        UnresolvedChannelCount -= 1;
                        UnresolvedTileCount -= 1;
                        break;

                    case Resolution.Empty:
                        UnresolvedEmptyCount -= 1;
                        UnresolvedTileCount -= 1;
                        break;
                }
                break;
        }
    }

    public override string ToString()
    {
        return $"Tableau (Size: {Grid.Size})";
    }
}
