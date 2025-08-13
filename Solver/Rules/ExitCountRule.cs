using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

public class ExitCountRule : IRule
{
    private readonly SolverGrid _grid;
    private readonly IReadOnlyList<UnorderedPair<Coordinates>> _cornerRadialCoordinates;

    public ExitCountRule(SolverGrid grid)
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
        // No unresolved border can be an exit
        foreach (var border in tableau.GetBorders())
        {
            border.TryResolve(Resolution.Empty, notifier, ResolutionReason.ExitCount);
        }
    }

    private void InvokeWithUnresolvedExits(Tableau tableau, INotifier notifier)
    {
        var exitSets = new List<ExitSet>();

        // Add single-element set for any already-resolved exit (either zwro or one).
        foreach (var termination in tableau.Thalweg.Exits)
        {
            exitSets.Add(new ExitSet(1, 1, [termination.Border]));
        }

        // Populate list with all sets of border edges that have at least one exit as determined by the aisle count,
        foreach (var aisle in tableau.Aisles.Values)
        {
            if (aisle.IsMargin)
            {
                // If the channel tile count is 1, 2 or 4 there must be at least one exit.
                switch (aisle.ChannelTileCount)
                {
                    case 1:
                        exitSets.Add(new ExitSet(1, 1, [.. aisle.Borders.Where(e => e.NormalAxis != aisle.Axis)]));
                        break;

                    case 2:
                    case 4:
                        exitSets.Add(new ExitSet(1, 2, [.. aisle.Borders]));
                        break;
                }
            }
            else
            {
                switch (aisle.ChannelTileCount)
                {
                    case 1:
                        exitSets.Add(new ExitSet(1, 1, [.. aisle.Borders]));
                        break;

                    case 3:
                        if (tableau.Aisles[(aisle.Axis, aisle.Index - 1)].ChannelTileCount > 0 &&
                            tableau.Aisles[(aisle.Axis, aisle.Index + 1)].ChannelTileCount > 0)
                        {
                            exitSets.Add(new ExitSet(1, 1, [.. aisle.Borders]));
                        }
                        break;
                }
            }
        }

        // Add adjacent borders with 3 adjacent thalweg segment terminations since there must be a single exit.
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
                var adjacentBorders = adjacentBorderTiles.SelectMany(t => t.Edges.Where(e => e.IsBorder));
                exitSets.Add(new ExitSet(1, 1, [..adjacentBorders]));
            }
        }

        if (exitSets.Count == 0)
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

            // Nothing more can be done.
            return;
        }

        var potentialExits = new Dictionary<Edge, Exit>();

        foreach (var exitSet in exitSets)
        {
            foreach (var border in exitSet.Borders)
            {
                if (!potentialExits.TryGetValue(border, out var potentialExit))
                {
                    potentialExit = new Exit(border);
                    potentialExits.Add(border, potentialExit);
                }

                potentialExit.ExitSets.Add(exitSet);
            }
        }

        var possibleExits = new HashSet<Edge>();

        foreach (var (exit1, exit2) in potentialExits.Values.GetAllPairs())
        {
            var commonExitSets = exit1.ExitSets.Intersect(exit2.ExitSets);
            if (commonExitSets.Any(exitSet => exitSet.MaxExits < 2))
            {
                continue;
            }

            if (exitSets.Except(exit1.ExitSets).Except(exit2.ExitSets).Any())
            {
                continue;
            }

            possibleExits.Add(exit1.Border);
            possibleExits.Add(exit2.Border);
        }

        if (possibleExits.Count == 2)
        {
            // Both exits are resolved
            foreach (var border in possibleExits)
            {
                border.TryResolve(Resolution.Channel, notifier, ResolutionReason.ExitCount);
            }

            // No remaining unresolved border can be an exit
            foreach (var border in tableau.GetBorders())
            {
                border.TryResolve(Resolution.Empty, notifier, ResolutionReason.ExitCount);
            }

            return;
        }

        if (potentialExits.Values.None(exit => exit.ExitSets.Count == exitSets.Count))
        {
            // No single potential exit can account for all the identified exit sets so both exits
            // must be in the possibleExits set and all borders not in this set can be marked as resolved.
            foreach (var border in tableau.GetBorders().Except(possibleExits))
            {
                border.TryResolve(Resolution.Empty, notifier, ResolutionReason.ExitCount);
            }
        }
        else
        {
            // Corner exit rule
            var cornerRadialEdges = _cornerRadialCoordinates.Select(coords =>
                tableau.Edges[coords]);

            foreach (var edge in cornerRadialEdges)
            {
                if (!edge.IsResolved &&
                    edge.Tiles.All(t =>
                        t.Resolution == Resolution.Channel &&
                        !t.Edges.Any(e => possibleExits.Contains(e))))
                {
                    edge.TryResolve(Resolution.Channel, notifier, ResolutionReason.CornerTileWithSinglePotentialExit);
                }
            }

            foreach (var aisle in tableau.GetMarginAisles())
            {
                if (!possibleExits.Overlaps(aisle.Borders))
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
    }

    public readonly struct Exit(Edge border)
    {
        public Edge Border { get; } = border;

        public HashSet<ExitSet> ExitSets { get; } = [];
    }

    public readonly struct ExitSet(int minExits, int maxExits, IEnumerable<Edge> borders)
    {
        public HashSet<Edge> Borders { get; } = [.. borders];

        public int MinExits { get; } = minExits;

        public int MaxExits { get; } = maxExits;
    }

    public override string ToString()
    {
        return nameof(ExitCountRule);
    }
}
