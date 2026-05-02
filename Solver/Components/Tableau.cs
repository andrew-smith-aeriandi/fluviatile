using Solver.Framework;
using System.Collections.Frozen;
using System.Text;

namespace Solver.Components;

public class Tableau : IComponent
{
    public Tableau(
        string tag,
        SolverGrid grid,
        Thalweg thalweg,
        IEnumerable<Aisle> aisles,
        IEnumerable<Tile> tiles,
        IEnumerable<Edge> edges)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(thalweg);
        ArgumentNullException.ThrowIfNull(aisles);
        ArgumentNullException.ThrowIfNull(tiles);
        ArgumentNullException.ThrowIfNull(edges);

        Tag = tag;
        Grid = grid;
        Thalweg = thalweg;
        Aisles = aisles.ToFrozenDictionary(aisle => aisle.GetDefaultKey());
        Tiles = tiles.ToFrozenDictionary(tile => tile.GetDefaultKey());
        Edges = edges.ToFrozenDictionary(edge => edge.GetDefaultKey());

        ChannelTileCounts = [.. aisles.Order(AisleComparer.Default).Select(a => a.ChannelTileCount)];
        ChannelTileCount = aisles.Where(aisle => aisle.Axis == Axis.X).Sum(aisle => aisle.ChannelTileCount);
        EmptyTileCount = TileCount - ChannelTileCount;
        ResolvedTileCount = tiles.Count(tile => tile.IsResolved);
        ResolvedChannelTileCount = tiles.Count(tile => tile.Resolution == Resolution.Channel);
        ResolvedEmptyTileCount = tiles.Count(tile => tile.Resolution == Resolution.Empty);
        ResolvedEdgeCount = edges.Count(edge => edge.IsResolved);
    }

    /// <summary>
    /// String identfier of the tableau 
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// List of the count of tiles containing channels in the aisles that comprise a grid   
    /// </summary>
    /// <remarks>
    /// The aisle counts are ordered by the aisle axis (X, Y, Z) and then by the aisle index.
    /// </remarks>
    public IReadOnlyList<int> ChannelTileCounts { get; }

    /// <summary>
    /// Count of tiles in the grid
    /// </summary>
    public int TileCount => Grid.TileCount;

    /// <summary>
    /// Count of resolved tiles in the grid 
    /// </summary>
    public int ResolvedTileCount { get; private set; }

    /// <summary>
    /// Count of unresolved tiles in the grid
    /// </summary>
    public int UnresolvedTileCount => Grid.TileCount - ResolvedTileCount;

    /// <summary>
    /// Count of tiles containing channels in the grid
    /// </summary>
    public int ChannelTileCount { get; }

    /// <summary>
    /// Count of resolved tiles containing channels in the grid 
    /// </summary>
    public int ResolvedChannelTileCount { get; private set; }

    /// <summary>
    /// Count of unresolved tiles containing channels in the grid 
    /// </summary>
    public int UnresolvedChannelTileCount => ChannelTileCount - ResolvedChannelTileCount;

    /// <summary>
    /// Count of empty tiles in the grid
    /// </summary>
    public int EmptyTileCount { get; }

    /// <summary>
    /// Count of resolved empty tiles in the grid
    /// </summary>
    public int ResolvedEmptyTileCount { get; private set; }

    /// <summary>
    /// Count of unresolved empty tiles in the grid
    /// </summary>
    public int UnresolvedEmptyTileCount => EmptyTileCount - ResolvedEmptyTileCount;

    /// <summary>
    /// Count of edges in the grid
    /// </summary>
    public int EdgeCount => Grid.EdgeCount;

    /// <summary>
    /// Count of resolved edges in the grid
    /// </summary>
    public int ResolvedEdgeCount { get; private set; }

    /// <summary>
    /// Count of unresolved edges in the grid
    /// </summary>
    public int UnresolvedEdgeCount => Grid.EdgeCount - ResolvedEdgeCount;

    /// <summary>
    /// Reference to the grid
    /// </summary>
    public SolverGrid Grid { get; }

    /// <summary>
    /// Reference to the resolved channel segments
    /// </summary>
    public Thalweg Thalweg { get; }

    /// <summary>
    /// Dictionary of the aisles comprising the grid, whose keys are the normal axis and index of each aisle 
    /// </summary>
    public FrozenDictionary<(Axis, int), Aisle> Aisles { get; }

    /// <summary>
    /// Dictionary of the tiles comprising the grid, whose keys are the coordinates of the centre of each tile
    /// </summary>
    public FrozenDictionary<Coordinates, Tile> Tiles { get; }

    /// <summary>
    /// Dictionary of the edges comprising the grid, whose keys are the cooordinate pairs specifying the vertices of each edge
    /// </summary>
    public FrozenDictionary<UnorderedPair<Coordinates>, Edge> Edges { get; }

    /// <summary>
    /// Updates the resolved counts (and implicitly the unresolved counts) for the grid and pertinent aisles
    /// whenever a tile or edge component is resolved
    /// </summary>
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
                        ResolvedChannelTileCount += 1;
                        ResolvedTileCount += 1;
                        break;

                    case Resolution.Empty:
                        ResolvedEmptyTileCount += 1;
                        ResolvedTileCount += 1;
                        break;
                }
                break;

            case Edge:
                ResolvedEdgeCount += 1;
                break;
        }
    }

    public string OutputState()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"{Grid}");
        builder.AppendLine($"{nameof(ChannelTileCounts)}: [{string.Join(',', ChannelTileCounts)}]");
        builder.AppendLine($"{nameof(TileCount)}: {TileCount}");
        builder.AppendLine($"{nameof(UnresolvedTileCount)}: {UnresolvedTileCount}");
        builder.AppendLine($"{nameof(ResolvedTileCount)}: {ResolvedTileCount}");
        builder.AppendLine($"{nameof(ChannelTileCount)}: {ChannelTileCount}");
        builder.AppendLine($"{nameof(UnresolvedChannelTileCount)}: {UnresolvedChannelTileCount}");
        builder.AppendLine($"{nameof(ResolvedChannelTileCount)}: {ResolvedChannelTileCount}");
        builder.AppendLine($"{nameof(EmptyTileCount)}: {EmptyTileCount}");
        builder.AppendLine($"{nameof(UnresolvedEmptyTileCount)}: {UnresolvedEmptyTileCount}");
        builder.AppendLine($"{nameof(ResolvedEmptyTileCount)}: {ResolvedEmptyTileCount}");

        builder.AppendLine("Tiles");
        foreach (var tile in Tiles.Values.Order(TileComparer.Default))
        {
            builder.AppendLine(tile.ToString());
        }

        builder.AppendLine("Edges");
        foreach (var edge in Edges.Values.Order(EdgeComparer.Default))
        {
            builder.AppendLine(edge.ToString());
        }

        builder.AppendLine("Aisles");
        foreach (var aisle in Aisles.Values.Order(AisleComparer.Default))
        {
            builder.AppendLine(aisle.ToString());
            builder.AppendLine($"{nameof(aisle.TileCount)}: {aisle.TileCount}");
            builder.AppendLine($"{nameof(aisle.ResolvedTileCount)}: {aisle.ResolvedTileCount}");
            builder.AppendLine($"{nameof(aisle.UnresolvedTileCount)}: {aisle.UnresolvedTileCount}");
            builder.AppendLine($"{nameof(aisle.EmptyTileCount)}: {aisle.EmptyTileCount}");
            builder.AppendLine($"{nameof(aisle.ResolvedEmptyTileCount)}: {aisle.ResolvedEmptyTileCount}");
            builder.AppendLine($"{nameof(aisle.UnresolvedEmptyTileCount)}: {aisle.UnresolvedEmptyTileCount}");
        }

        builder.AppendLine("Thalweg");
        builder.AppendLine($"{nameof(Thalweg.ChannelTileCount)}: {Thalweg.ChannelTileCount}");
        builder.AppendLine($"{nameof(Thalweg.LinkedChannelTileCount)}: {Thalweg.LinkedChannelTileCount}");
        builder.AppendLine($"{nameof(Thalweg.UnlinkedChannelTileCount)}: {Thalweg.UnlinkedChannelTileCount}");
        builder.AppendLine("Exits");
        builder.AppendLine($"{nameof(Thalweg.TerminationCount)}: {Thalweg.TerminationCount}");
        builder.AppendLine($"{nameof(Thalweg.ResolvedTerminationCount)}: {Thalweg.ResolvedTerminationCount}");
        builder.AppendLine($"{nameof(Thalweg.UnresolvedTerminationCount)}: {Thalweg.UnresolvedTerminationCount}");
        foreach (var exit in Thalweg.Terminations)
        {
            builder.AppendLine(exit.ToString());
        }
        builder.AppendLine("Thalweg Segments");
        builder.AppendLine($"{nameof(Thalweg.SegmentCount)}: {Thalweg.SegmentCount}");
        foreach (var segment in Thalweg.Segments)
        {
            builder.AppendLine(segment.ToString());
        }

        return builder.ToString();
    }

    public override string ToString()
    {
        return $"{Tag}:[{string.Join(',', ChannelTileCounts.Select(count => count.ToString()))}]";
    }
}
