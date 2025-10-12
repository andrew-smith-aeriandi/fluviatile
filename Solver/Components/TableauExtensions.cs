using Fluviatile.Grid;
using Solver.Framework;

namespace Solver.Components;

public static class TableauExtensions
{
    public static Tableau Clone(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return new Tableau(
            tableau.Grid,
            tableau.Thalweg.Clone(),
            tableau.GetAisles(),
            tableau.GetTiles(),
            tableau.GetEdges());
    }

    public static bool IsSolved(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return tableau.Thalweg.UnlinkedTileCount == 0 &&
            tableau.Thalweg.UnresolvedExitCount == 0 &&
            tableau.Thalweg.SegmentCount == 1;
    }

    public static IEnumerable<Aisle> GetAisles(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        foreach (var (_, aisle) in tableau.Aisles)
        {
            yield return aisle;
        }
    }

    public static IEnumerable<Aisle> GetAisles(this Tableau tableau, Axis axis)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        for (var index = 0; index < tableau.Grid.AisleCountPerAxis; index++)
        {
            yield return tableau.Aisles[(axis, index)];
        }
    }

    public static IEnumerable<Aisle> GetMarginAisles(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        var maxIndex = tableau.Grid.AisleCountPerAxis - 1;

        foreach (var axis in SolverGrid.Axes)
        {
            yield return tableau.Aisles[(axis, 0)];
            yield return tableau.Aisles[(axis, maxIndex)];
        }
    }

    public static IEnumerable<Aisle> GetMarginAisles(this Tableau tableau, Axis axis)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        var maxIndex = tableau.Grid.AisleCountPerAxis - 1;

        yield return tableau.Aisles[(axis, 0)];
        yield return tableau.Aisles[(axis, maxIndex)];
    }

    public static bool TryGetAisle(this Tableau tableau, Axis axis, int index, out Aisle? aisle)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return tableau.Aisles.TryGetValue((axis, index), out aisle);
    }

    public static IEnumerable<Tile> GetTiles(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        foreach (var (_, tile) in tableau.Tiles)
        {
            yield return tile;
        }
    }

    public static bool TryGetTile(this Tableau tableau, Framework.Coordinates coordinates, out Tile? tile)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return tableau.Tiles.TryGetValue(coordinates, out tile);
    }

    public static IEnumerable<Edge> GetEdges(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        foreach (var (_, edge) in tableau.Edges)
        {
            yield return edge;
        }
    }

    public static IEnumerable<Edge> GetBorders(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        foreach (var (_, edge) in tableau.Edges)
        {
            if (edge.IsBorder)
            {
                yield return edge;
            }
        }
    }

    public static bool TryGetEdge(this Tableau tableau, Tile tile, Axis axis, out Edge? edge)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return tableau.Edges.TryGetValue(tile.GetEdgeKey(axis), out edge);
    }

    public static IEnumerable<NodeState> GetNodeState(this Tableau tableau)
    {
        foreach (var (coordinates, tile) in tableau.Tiles)
        {
            switch (tile.Resolution)
            {
                case Resolution.Unknown:
                    yield return new(coordinates.X, coordinates.Y, 0);
                    break;

                case Resolution.Empty:
                    yield return new(coordinates.X, coordinates.Y, 256);
                    break;

                case Resolution.Channel:
                    var mask = tile.Edges
                        .Where(e => e.Resolution == Resolution.Channel)
                        .Aggregate(1, (m, edge) => m | (tile.Orientation, edge.NormalAxis) switch
                        {
                            (Orientation.Up, Axis.X) => 2,
                            (Orientation.Up, Axis.Y) => 4,
                            (Orientation.Up, Axis.Z) => 8,
                            (Orientation.Down, Axis.X) => 16,
                            (Orientation.Down, Axis.Y) => 32,
                            (Orientation.Down, Axis.Z) => 64,
                            _ => 0
                        });

                    yield return new(coordinates.X, coordinates.Y, mask);
                    break;
            }
        }
    }
}
