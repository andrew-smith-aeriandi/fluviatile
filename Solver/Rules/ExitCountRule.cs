using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

public class ExitCountRule : IRule
{
    private readonly Grid _grid;
    private readonly IReadOnlyList<UnorderedPair<Coordinates>> _cornerRadialCoordinates;

    public ExitCountRule(Grid grid)
    {
        ArgumentNullException.ThrowIfNull(grid);

        _grid = grid;
        var radius1 = grid.Radius;
        var radius2 = grid.CoordinateLength(^1);

        _cornerRadialCoordinates =
        [
            new(new Coordinates(-radius1, 0), new Coordinates(-radius2, 0)),
            new(new Coordinates(0, -radius1), new Coordinates(0, -radius2)),
            new(new Coordinates(radius1, -radius1), new Coordinates(radius2, -radius2)),
            new(new Coordinates(radius1, 0), new Coordinates(radius2, 0)),
            new(new Coordinates(0, radius1), new Coordinates(0, radius2)),
            new(new Coordinates(-radius1, radius1), new Coordinates(-radius2, radius2))
        ];
    }

    public IEnumerable<Type> GetPertinentComponents()
    {
        yield return typeof(Tableau);
    }

    public void Invoke(IComponent component, INotifier notifier)
    {
        switch (component)
        {
            case Tableau tableau:
                if (tableau.Thalweg.UnresolvedExitCount == 0)
                {
                    InvokeWithAllExitsResolved(tableau, notifier);
                }
                else
                {
                    InvokeWithUnresolvedExits(tableau, notifier);
                }
                break;
        }
    }

    private void InvokeWithAllExitsResolved(Tableau tableau, INotifier notifier)
    {
        foreach (var border in tableau.GetBorders())
        {
            border.TryResolve(Resolution.Empty, notifier, ResolutionReason.ExitCount);
        }
    }

    private void InvokeWithUnresolvedExits(Tableau tableau, INotifier notifier)
    {
        // Populate list with all sets of border edges that have at least one exit as determined by the aisle count,
        // plus single-element sets for any already-resolved exits.
        var sets = tableau.Thalweg.Exits
            .Select(termination => new HashSet<Edge>([termination.Border]))
            .ToList();

        // NOTE: if the channel tile count is 1, 2 or 4 there must be at laest one exit regardless of the grid size.
        foreach (var aisle in tableau.Aisles.Values.Where(a => a.IsMargin))
        {
            switch (aisle.ChannelTileCount)
            {
                case 1:
                    sets.Add([.. aisle.Borders.Where(e => e.NormalAxis != aisle.Axis)]);
                    break;

                case 2:
                case 4:
                    sets.Add([.. aisle.Borders]);
                    break;
            }
        }

        var borderTiles = new HashSet<Tile>();
        var interiorTiles = new HashSet<Tile>();
        foreach (var segment in tableau.Thalweg.Segments)
        {
            if (segment.First is Tile firstTile)
            {
                if (firstTile.HasBorder)
                {
                    borderTiles.Add(firstTile);
                }
                else
                {
                    interiorTiles.Add(firstTile);
                }
            }

            if (segment.Last is Tile lastTile)
            {
                if (lastTile.HasBorder)
                {
                    borderTiles.Add(lastTile);
                }
                else
                {
                    interiorTiles.Add(lastTile);
                }
            }
        }

        foreach (var interiorTile in interiorTiles)
        {
            var adjacentBorderTiles = interiorTile.GetPotentiallyLinkableTiles()
                .Intersect(borderTiles)
                .Where(t => t.Edges.None(e => e.IsBorder && e.IsResolved))
                .ToArray();

            if (adjacentBorderTiles.Length == 2)
            {
                sets.Add([.. adjacentBorderTiles.SelectMany(t => t.Edges.Where(e => e.IsBorder))]);
            }
        }

        // Reduce sets by mapping overlapping pairs of sets to their intersections
        // until the collection of sets includes only non-overlapping sets.
        while (true)
        {
            var touchedSets = new HashSet<HashSet<Edge>>();
            var updatedSets = new List<HashSet<Edge>>();

            foreach (var (set1, set2) in sets.GetAllPairs())
            {
                if (set1.Overlaps(set2))
                {
                    touchedSets.Add(set1);
                    touchedSets.Add(set2);

                    if (set1.IsSubsetOf(set2))
                    {
                        updatedSets.Add(set1);
                    }
                    else if (set2.IsSubsetOf(set1))
                    {
                        updatedSets.Add(set2);
                    }
                    else
                    {
                        updatedSets.Add([.. set1.Intersect(set2)]);
                    }
                }
            }

            if (touchedSets.Count == 0)
            {
                // Reduction is complete when no pairs of sets overlap
                break;
            }

            // Include any sets that do not overlap with other sets
            updatedSets.AddRange(sets.Except(touchedSets));
            sets = updatedSets;
        }

        var bordersWithPotentialExits = new HashSet<Edge>();
        sets.InvokeForEach(bordersWithPotentialExits.UnionWith);

        switch (sets.Count)
        {
            case 2:
                {
                    // We have accounted for all exits within non-overlapping sets of borders,
                    // so all other borders must be empty.
                    foreach (var border in tableau.GetBorders())
                    {
                        if (!bordersWithPotentialExits.Contains(border))
                        {
                            border.TryResolve(Resolution.Empty, notifier, ResolutionReason.ExitCount);
                        }
                    }
                }
                break;

            case 1:
                {
                    // Corner exit rule
                    var cornerRadialEdges = _cornerRadialCoordinates.Select(coords =>
                        tableau.Edges[coords]);

                    foreach (var edge in cornerRadialEdges)
                    {
                        if (!edge.IsResolved &&
                            edge.Tiles.All(t =>
                                t.Resolution == Resolution.Channel &&
                                !t.Edges.Any(e => bordersWithPotentialExits.Contains(e))))
                        {
                            edge.TryResolve(Resolution.Channel, notifier, ResolutionReason.CornerTileWithSinglePotentialExit);
                        }
                    }

                    foreach (var aisle in tableau.Aisles.Values.Where(a => a.IsMargin))
                    {
                        if (!bordersWithPotentialExits.Overlaps(aisle.Borders))
                        {
                            // If there must be an exit that does not overlap with the aisle borders then we
                            // can resolve some aisle borders for specific channel tile counts.
                            switch (aisle.ChannelTileCount)
                            {
                                case 1:
                                case 3:
                                    foreach (var border in aisle.Borders.Where(e => e.NormalAxis == aisle.Axis))
                                    {
                                        border.TryResolve(Resolution.Empty, notifier, ResolutionReason.BorderAisleCountWithSinglePotentialExit);
                                    }
                                    break;

                                case 2:
                                    foreach (var border in aisle.Borders.Where(e => e.NormalAxis != aisle.Axis))
                                    {
                                        border.TryResolve(Resolution.Empty, notifier, ResolutionReason.BorderAisleCountWithSinglePotentialExit);
                                    }
                                    break;

                                case 6:
                                    foreach (var border in aisle.Borders.Where(e => e.NormalAxis == aisle.Axis))
                                    {
                                        var aisleIndex = aisle.Tiles.IndexOf(border.Tiles.Single());
                                        if (aisleIndex >= 0 && Math.Max(aisleIndex + 1, aisle.TileCount - aisleIndex) < 6)
                                        {
                                            border.TryResolve(Resolution.Empty, notifier, ResolutionReason.BorderAisleCountWithSinglePotentialExit);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
                break;

            case 0:
                {
                    foreach (var aisle in tableau.GetMarginAisles())
                    {
                        switch (aisle.ChannelTileCount)
                        {
                            case 1:
                                foreach (var border in aisle.Borders.Where(e => e.NormalAxis == aisle.Axis))
                                {
                                    border.TryResolve(Resolution.Empty, notifier, ResolutionReason.BorderAisleCount);
                                }
                                break;
                        }
                    }
                }
                break;
        }
    }

    public override string ToString()
    {
        return nameof(ExitCountRule);
    }
}
