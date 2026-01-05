using Solver.Components;
using Solver.Framework;
using Tableau = Solver.Components.Tableau;

namespace Solver.Rules;

public class TarjansRule(Tableau tableau) : Rule(tableau)
{
    public override string ToString()
    {
        return nameof(TarjansRule);
    }

    public override IEnumerable<Type> GetPertinentComponents()
    {
        yield return typeof(Tableau);
    }

    public override void Invoke(IComponent component, INotifier notifier)
    {
        switch (component)
        {
            case Tableau tableau:
                InvokeInternal(tableau, notifier);
                break;
        }
    }

    private static void InvokeInternal(Tableau tableau, INotifier notifier)
    {
        var tiles = new List<Tile>();
        var adjacency = new List<(Tile, Tile)>();

        foreach (var tile in tableau.GetTiles())
        {
            switch (tile.Resolution)
            {
                case Resolution.Unknown:
                    tiles.Add(tile);
                    foreach (var linkableTile in tile.GetPotentiallyLinkableTiles())
                    {
                        adjacency.Add((tile, linkableTile));
                    }
                    break;

                case Resolution.Channel:
                    if (tableau.Thalweg.TryGetSegment(tile, out var segment) && segment is not null)
                    {
                        if (!segment.IsTermination(tile))
                        {
                            break;
                        }

                        tiles.Add(tile);

                        if ((tile == segment.Last ? segment.First : segment.Last) is Tile otherTile)
                        {
                            adjacency.Add((tile, otherTile));
                        }

                        foreach (var linkableTile in tile.Edges
                            .Where(e => !e.IsResolved)
                            .SelectMany(e => e.Tiles.Where(t => t != tile && t.Resolution != Resolution.Empty)))
                        {
                            adjacency.Add((tile, linkableTile));
                        }
                    }
                    else
                    {
                        tiles.Add(tile);
                        foreach (var linkableTile in tile.GetPotentiallyLinkableTiles())
                        {
                            adjacency.Add((tile, linkableTile));
                        }
                    }
                    break;
            }
        }

        // Use Tarjan's algorithm to find articulation points that have unresolved tiles or edeges, i.e. tiles that if removed
        // would divide the graph into multiple connected graphs (channels), which would not be a valid solution.
        var bridgeTiles = Tarjan
            .GetArticulationPoints(tiles, adjacency)
            .ToArray();

        if (bridgeTiles.Length == 0)
        {
            return;
        }

        var unresolvedEmptyTileCounts = GetUnresolvedEmptyTileCounts(tableau);

        foreach (var bridgeTile in bridgeTiles)
        {
            var linkableComponents = bridgeTile.GetPotentiallyLinkableComponents().ToArray();

            if (linkableComponents.Length == 2)
            {
                if (linkableComponents.All(tuple =>
                    RequiresChannel(
                        tableau.Grid,
                        unresolvedEmptyTileCounts,
                        bridgeTile,
                        tuple.Tile,
                        tuple.Edge)))
                {
                    bridgeTile.TryResolve(Resolution.Channel, notifier, ResolutionReason.TarjansAlgorithm);
                    foreach (var (tile, edge) in linkableComponents)
                    {
                        tile.TryResolve(Resolution.Channel, notifier, ResolutionReason.TarjansAlgorithm);
                        edge.TryResolve(Resolution.Channel, notifier, ResolutionReason.TarjansAlgorithm);
                    }
                }
            }
        }
    }

    private static bool RequiresChannel(
        SolverGrid grid,
        int[] unresolvedEmptyTileCounts,
        Tile startTile,
        Tile nextTile,
        Edge commonEdge)
    {
        switch (commonEdge.Resolution)
        {
            case Resolution.Channel:
                return true;
            case Resolution.Empty:
                return false;
        }

        var aisleCounts = unresolvedEmptyTileCounts.ToArray();
        var visited = new HashSet<Tile>
        {
            startTile
        };

        var queue = new Queue<Tile>();
        queue.Enqueue(nextTile);

        while (queue.TryDequeue(out var tile))
        {
            switch (tile.Resolution)
            {
                case Resolution.Channel:
                    return true;

                case Resolution.Empty:
                    foreach (var aisle in tile.Aisles)
                    {
                        var ordinal = grid.AisleCountOrdinal(aisle.Axis, aisle.Index);
                        var aisleCount = aisleCounts[ordinal];
                        if (aisleCount == 0)
                        {
                            return true;
                        }

                        aisleCounts[ordinal] = aisleCount - 1;
                    }
                    break;
            }

            foreach (var linkableTile in tile.GetPotentiallyLinkableTiles())
            {
                if (visited.Add(linkableTile))
                {
                    queue.Enqueue(linkableTile);
                }
            }
        }

        return false;
    }

    private static int[] GetUnresolvedEmptyTileCounts(Tableau tableau)
    {
        var grid = tableau.Grid;
        var counts = new int[grid.AisleCount];

        foreach (var aisle in tableau.GetAisles())
        {
            var ordinal = grid.AisleCountOrdinal(aisle.Axis, aisle.Index);
            counts[ordinal] = aisle.UnresolvedEmptyTileCount;
        }

        return counts;
    }
}
