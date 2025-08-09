using Solver.Components;
using System.Diagnostics;
using static Solver.Framework.LinqExtensions;

namespace Solver.Framework;

public class TableauFactory
{
    public Tableau Create(Grid grid, IEnumerable<int> counts)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(counts);

        var channelCounts = counts.ToArray();
        if (channelCounts.Length != grid.AisleCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(counts),
                $"Colllection must contain {grid.AisleCount} elements.");
        }

        var axisChannelCounts = channelCounts
            .Chunk(grid.AisleCountPerAxis)
            .Select(chunk => chunk.Sum())
            .ToArray();

        var channelTileCount = axisChannelCounts[0];

        if (!axisChannelCounts.Skip(1).All(count => count == channelTileCount))
        {
            throw new ArgumentException(
                "Sum of channel tile counts for each axis must be equal.",
                nameof(counts));
        }

        var aisles = new HashSet<Aisle>(AisleComparer.Default);
        var tiles = new HashSet<Tile>(TileComparer.Default);
        var edges = new HashSet<Edge>(EdgeComparer.Default);

        var inputIndex = 0;
        foreach (var axis in Grid.Axes)
        {
            for (var index = 0; index < grid.AisleCountPerAxis; index++)
            {
                var aisle = new Aisle(
                    axis,
                    index,
                    index == 0 || index == grid.AisleCountPerAxis - 1,
                    grid.AisleTileCount(index),
                    channelCounts[inputIndex++]);

                aisles.Add(aisle);
            }
        }

        for (var aisleXIndex = 0; aisleXIndex < grid.AisleCountPerAxis; aisleXIndex++)
        {
            for (var aisleYIndex = 0; aisleYIndex < grid.AisleCountPerAxis; aisleYIndex++)
            {
                if (grid.TryGetTileCentreCoordinates(
                    Orientation.Down,
                    aisleXIndex,
                    aisleYIndex,
                    out var centreDown))
                {
                    var tileDown = new Tile(centreDown, Grid.ShapeDown);
                    edges.UnionWith(grid.CreateEdgesFromVertices(tileDown.Vertices));
                    tiles.Add(tileDown);
                }

                if (grid.TryGetTileCentreCoordinates(
                    Orientation.Up,
                    aisleXIndex,
                    aisleYIndex,
                    out var centreUp))
                {
                    var tileUp = new Tile(centreUp, Grid.ShapeUp);
                    edges.UnionWith(grid.CreateEdgesFromVertices(tileUp.Vertices));
                    tiles.Add(tileUp);
                }
            }
        }

        var thalweg = new Thalweg(grid, channelTileCount);
        var tableau = new Tableau(grid, thalweg, aisles, tiles, edges);

        foreach (var tile in tiles)
        {
            var tileAisles = grid.GetAisleKeys(tile)
                .Select(key => tableau.Aisles.GetValueOrDefault(key))
                .OfType<Aisle>();

            var tileEdges = tile.Vertices
                .SelectWithNext((v1, v2) => new UnorderedPair<Coordinates>(v1, v2), SelectWithNextOption.LoopBackToStart)
                .Select(pair => tableau.Edges.GetValueOrDefault(pair))
                .OfType<Edge>();

            tile.SetAisles(tileAisles);
            tile.SetEdges(tileEdges);
            tile.Freeze();
        }

        foreach (var edge in edges)
        {
            var v0 = edge.Vertices[0];
            var v1 = edge.Vertices[1];

            var (keyMinus, keyPlus) = edge.NormalAxis switch
            {
                Axis.X => (
                    (v0.Y <= v1.Y ? v0 : v1).Add(-Grid.OneThird, Grid.TwoThird),
                    (v1.Y >= v0.Y ? v1 : v0).Add(Grid.OneThird, -Grid.TwoThird)),
                Axis.Y => (
                    (v0.Z <= v1.Z ? v0 : v1).Add(-Grid.OneThird, -Grid.OneThird),
                    (v1.Z >= v0.Z ? v1 : v0).Add(Grid.OneThird, Grid.OneThird)),
                Axis.Z => (
                    (v0.X <= v1.X ? v0 : v1).Add(Grid.TwoThird, -Grid.OneThird),
                    (v1.X >= v0.X ? v1 : v0).Add(-Grid.TwoThird, Grid.OneThird)),
                _ => throw new UnreachableException($"Unsupported axis: {edge.NormalAxis}")
            };

            var tileMinus = tableau.Tiles.GetValueOrDefault(keyMinus);
            var tilePlus = tableau.Tiles.GetValueOrDefault(keyPlus);

            edge.SetTiles(tileMinus, tilePlus);
            edge.Freeze();
        }

        foreach (var aisle in aisles)
        {
            var aisleTiles = tableau.Tiles.Values
                .Where(tile => tile.Aisles.Contains(aisle));

            aisle.SetTiles(aisleTiles);
            aisle.Freeze();
        }

        return tableau;
    }
}
