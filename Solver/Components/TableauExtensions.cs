using Fluviatile.Grid;
using Solver.Framework;
using System.Diagnostics;
using Coordinates = Solver.Framework.Coordinates;

namespace Solver.Components;

public static class TableauExtensions
{
    public static IComponent GetEquivalentComponent(
        this Tableau tableau,
        IComponent alienComponent)
    {
        ArgumentNullException.ThrowIfNull(tableau);
        ArgumentNullException.ThrowIfNull(alienComponent);

        return alienComponent switch
        {
            Tableau => tableau,
            Thalweg => tableau.Thalweg,
            Aisle alienAisle => tableau.Aisles[alienAisle.GetDefaultKey()],
            Tile alienTile => tableau.Tiles[alienTile.GetDefaultKey()],
            Edge alienEdge => tableau.Edges[alienEdge.GetDefaultKey()],
            _ => throw new UnreachableException($"Unexpected type: {alienComponent.GetType().Name}")
        };
    }

    public static bool TryGetEquivalentComponent(
        this Tableau tableau,
        IComponent? alienComponent,
        out IComponent? component)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        switch (alienComponent)
        {
            case Tableau:
                component = tableau;
                return true;

            case Thalweg:
                component = tableau.Thalweg;
                return true;

            case Aisle alienAisle:
                if (tableau.Aisles.TryGetValue(alienAisle.GetDefaultKey(), out var aisle))
                {
                    component = aisle;
                    return true;
                }
                break;

            case Tile alienTile:
                if (tableau.Tiles.TryGetValue(alienTile.GetDefaultKey(), out var tile))
                {
                    component = tile;
                    return true;
                }
                break;

            case Edge alienEdge:
                if (tableau.Edges.TryGetValue(alienEdge.GetDefaultKey(), out var edge))
                {
                    component = edge;
                    return true;
                }
                break;
        }

        component = default;
        return false;
    }

    public static bool IsSolved(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return tableau.UnresolvedTileCount == 0 &&
            tableau.UnresolvedEdgeCount == 0 &&
            tableau.Thalweg.UnlinkedChannelTileCount == 0 &&
            tableau.Thalweg.UnresolvedTerminationCount == 0 &&
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

    public static bool TryGetTile(this Tableau tableau, Coordinates coordinates, out Tile? tile)
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

    public static bool TryGetUnresolvedComponent(this Tableau tableau, out IComponent? component)
    {
        component = tableau.Tiles.Values.FirstOrDefault(tile => !tile.IsResolved);
        return component is not null;
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

    public static IEnumerable<IResolvableComponent> GetThalwegHypotheticals(
        this Tableau tableau)
    {
        foreach (var segment in tableau.Thalweg.Segments)
        {
            foreach (var tile in segment.Ends.OfType<Tile>())
            {
                if (tile.Edges.FirstOrDefault(edge => !edge.IsResolved) is Edge unresolvedEdge)
                {
                    yield return unresolvedEdge;
                }
            }
        }
    }

    // TODO: implement properly
    public static IEnumerable<IResolvableComponent> GetHypotheticals(
        this Tableau tableau)
    {
        return tableau.GetThalwegHypotheticals()
            .Concat(tableau.Tiles.Values.OfType<IResolvableComponent>())
            .Concat(tableau.Edges.Values.OfType<IResolvableComponent>())
            .Where(c => !c.IsResolved);
    }
}
