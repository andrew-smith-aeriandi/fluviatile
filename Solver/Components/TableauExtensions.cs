using Fluviatile.Grid;
using Solver.Framework;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Coordinates = Solver.Framework.Coordinates;

namespace Solver.Components;

public static class TableauExtensions
{
    /// <summary>
    /// Returns the component in the tableau that is of the same type and in the same relative
    /// position as the specified component (typically from a distinct copy of the tableau)
    /// </summary>
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

    /// <summary>
    /// Attempts to return the component in the tableau that is of the same type and in the same relative
    /// position as the specified component (typically from a distinct copy of the tableau)
    /// </summary>
    public static bool TryGetEquivalentComponent(
        this Tableau tableau,
        IComponent? alienComponent,
        [MaybeNullWhen(false)] out IComponent component)
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

    /// <summary>
    /// Indicates whether the grid is considered to be solved
    /// </summary>
    public static bool IsSolved(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return tableau.UnresolvedTileCount == 0 &&
            tableau.UnresolvedEdgeCount == 0 &&
            tableau.Thalweg.UnlinkedChannelTileCount == 0 &&
            tableau.Thalweg.UnresolvedTerminationCount == 0 &&
            tableau.Thalweg.SegmentCount == 1;
    }

    /// <summary>
    /// Return all the aisles in the grid
    /// </summary>
    public static IEnumerable<Aisle> GetAisles(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        foreach (var (_, aisle) in tableau.Aisles)
        {
            yield return aisle;
        }
    }

    /// <summary>
    /// Return all the aisles in the grid that are normal to the specified axis
    /// </summary>
    public static IEnumerable<Aisle> GetAisles(this Tableau tableau, Axis axis)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        for (var index = 0; index < tableau.Grid.AisleCountPerAxis; index++)
        {
            yield return tableau.Aisles[(axis, index)];
        }
    }

    /// <summary>
    /// Return collection of aisles that comprise the tiles along the margin of the grid
    /// </summary>
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

    /// <summary>
    /// Return the pair of aisles on the margin of the grid normal to the specified axis
    /// </summary>
    public static IEnumerable<Aisle> GetMarginAisles(this Tableau tableau, Axis axis)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        var maxIndex = tableau.Grid.AisleCountPerAxis - 1;

        yield return tableau.Aisles[(axis, 0)];
        yield return tableau.Aisles[(axis, maxIndex)];
    }

    public static bool TryGetAisle(
        this Tableau tableau,
        Axis axis, int index,
        [MaybeNullWhen(false)] out Aisle aisle)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return tableau.Aisles.TryGetValue((axis, index), out aisle);
    }

    public static bool TryGetProximalAisle(
        this Tableau tableau,
        Aisle aisle,
        [MaybeNullWhen(false)] out Aisle proximalAisle)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        if (aisle.Index.IsInRangeExclusiveUpper(0, tableau.Grid.Size - 1))
        {
            proximalAisle = tableau.Aisles[(aisle.Axis, aisle.Index + 1)];
            return true;
        }
        else if (aisle.Index.IsInRangeExclusive(tableau.Grid.Size, tableau.Grid.AisleCountPerAxis))
        {
            proximalAisle = tableau.Aisles[(aisle.Axis, aisle.Index - 1)];
            return true;
        }

        proximalAisle = null;
        return false;
    }

    public static bool TryGetDistalAisle(
        this Tableau tableau,
        Aisle aisle,
        [MaybeNullWhen(false)] out Aisle distalAisle)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        if (aisle.Index.IsInRangeExclusive(0, tableau.Grid.Size))
        {
            distalAisle = tableau.Aisles[(aisle.Axis, aisle.Index - 1)];
            return true;
        }
        else if (aisle.Index.IsInRangeExclusiveUpper(tableau.Grid.Size, tableau.Grid.AisleCountPerAxis - 1))
        {
            distalAisle = tableau.Aisles[(aisle.Axis, aisle.Index + 1)];
            return true;
        }

        distalAisle = null;
        return false;
    }

    /// <summary>
    /// Enumerates all tiles in the tableau
    /// </summary>
    public static IEnumerable<Tile> GetTiles(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        foreach (var (_, tile) in tableau.Tiles)
        {
            yield return tile;
        }
    }

    /// <summary>
    /// Attempts to return a tile in the tableau based on its coordinates
    /// </summary>
    public static bool TryGetTile(
        this Tableau tableau,
        Coordinates coordinates,
        [MaybeNullWhen(false)] out Tile tile)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return tableau.Tiles.TryGetValue(coordinates, out tile);
    }

    /// <summary>
    /// Enumerates all edges in the tableau
    /// </summary>
    public static IEnumerable<Edge> GetEdges(this Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        foreach (var (_, edge) in tableau.Edges)
        {
            yield return edge;
        }
    }

    /// <summary>
    /// Enumerates all border edges in the tableau
    /// </summary>
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

    public static bool TryGetEdge(
        this Tableau tableau,
        Tile tile,
        Axis axis,
        [MaybeNullWhen(false)] out Edge edge)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return tableau.Edges.TryGetValue(tile.GetEdgeKey(axis), out edge);
    }

    public static bool TryGetUnresolvedComponent(
        this Tableau tableau,
        [MaybeNullWhen(false)] out IComponent component)
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
