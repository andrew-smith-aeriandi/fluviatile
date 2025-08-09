using Solver.Framework;

namespace Solver.Components;

public static class TableauExtensions
{
    public static bool IsSolved(this Tableau tableau)
    {
        return tableau.Thalweg.UnlinkedTileCount == 0 &&
            tableau.Thalweg.UnresolvedExitCount == 0 &&
            tableau.Thalweg.SegmentCount == 1;
    }

    public static bool TryGetAisle(this Tableau tableau, Axis axis, int index, out Aisle? aisle)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return tableau.Aisles.TryGetValue((axis, index), out aisle);
    }

    public static IEnumerable<Aisle> GetAisles(this Tableau tableau)
    {
        foreach (var (_, aisle) in tableau.Aisles)
        {
            yield return aisle;
        }
    }

    public static IEnumerable<Aisle> GetAisles(this Tableau tableau, Axis axis)
    {
        for (var index = 0; index < tableau.Grid.AisleCountPerAxis; index++)
        {
            yield return tableau.Aisles[(axis, index)];
        }
    }

    public static IEnumerable<Aisle> GetMarginAisles(this Tableau tableau)
    {
        var maxIndex = tableau.Grid.AisleCountPerAxis - 1;

        foreach (var axis in Grid.Axes)
        {
            yield return tableau.Aisles[(axis, 0)];
            yield return tableau.Aisles[(axis, maxIndex)];
        }
    }

    public static IEnumerable<Aisle> GetMarginAisles(this Tableau tableau, Axis axis)
    {
        var maxIndex = tableau.Grid.AisleCountPerAxis - 1;

        yield return tableau.Aisles[(axis, 0)];
        yield return tableau.Aisles[(axis, maxIndex)];
    }

    public static IEnumerable<Edge> GetBorders(this Tableau tableau)
    {
        return tableau.Edges.Values.Where(edge => edge.IsBorder);
    }

    public static bool TryGetEdge(this Tableau tableau, Tile tile, Axis axis, out Edge? edge)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return tableau.Edges.TryGetValue(tile.GetEdgeKey(axis), out edge);
    }
}

