using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

[RulePrioriry(QueuePriority.Default)]
public class ExitCountRule : Rule
{
    private readonly IReadOnlyList<UnorderedPair<Coordinates>> _cornerRadialCoordinates;

    public ExitCountRule(Tableau tableau) : base(tableau)
    {
        var radius1 = tableau.Grid.Radius;
        var radius2 = tableau.Grid.CoordinateLength(^1);

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

    public override string ToString()
    {
        return nameof(ExitCountRule);
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

    private void InvokeInternal(Tableau tableau, INotifier notifier)
    {
        if (tableau.Thalweg.UnresolvedTerminationCount == 0)
        {
            // All exits have already been resolved, so no unresolved border can be an exit.
            foreach (var border in tableau.GetBorders())
            {
                border.TryResolve(Resolution.Empty, notifier, ResolutionReason.Housekeeping);
            }

            return;
        }

        var exitSets = new List<ExitSet>();

        // Add single-element set for any already-resolved exit.
        // In practice, there will be either zero or one resolved exit.
        foreach (var termination in tableau.Thalweg.Terminations)
        {
            exitSets.Add(new ExitSet(1, 1, [termination.Border]));
        }

        // Populate list with all sets of border edges that have at least one exit as determined by the aisle count.
        foreach (var aisle in tableau.GetAisles())
        {
            if (aisle.IsMargin)
            {
                var proximalAisleChannelTileCount = tableau.TryGetProximalAisle(aisle, out var proximalAisle)
                    ? proximalAisle.ChannelTileCount
                    : 0;

                switch (aisle.ChannelTileCount)
                {
                    case 1:
                        if (proximalAisleChannelTileCount >= 2)
                        {
                            // One of the 2 lateral borders must be an exit
                            exitSets.Add(new ExitSet(1, 1, aisle.LateralBorders()));
                        }
                        break;

                    case 2:
                        if (proximalAisleChannelTileCount == 0)
                        {
                            // Both exits must be at one of the corners of the grid in this aisle
                            exitSets.Add(new ExitSet(1, 1, aisle.LateralBorders()));
                            exitSets.Add(new ExitSet(1, 1, [aisle.Borders[1], aisle.Borders[^2]]));
                        }
                        else if (proximalAisleChannelTileCount == 2 || proximalAisleChannelTileCount == 3)
                        {
                            // One of the normal borders must be an exit
                            exitSets.Add(new ExitSet(1, 1, aisle.NormalBorders()));
                        }
                        else if (proximalAisleChannelTileCount >= 4)
                        {
                            // Either one normal exit or 2 lateral exits
                            exitSets.Add(new ExitSet(1, 2, aisle.Borders));
                        }
                        break;

                    case 3:
                        if (proximalAisleChannelTileCount == 0)
                        {
                            // 2 normal exits
                            exitSets.Add(new ExitSet(2, 2, aisle.NormalBorders()));
                        }
                        else if (proximalAisleChannelTileCount == 2 || proximalAisleChannelTileCount == 3)
                        {
                            // One of the lateral borders must be an exit
                            exitSets.Add(new ExitSet(1, 1, aisle.LateralBorders()));
                        }
                        break;

                    case 4:
                        if (proximalAisleChannelTileCount == 0)
                        {
                            // Normal exit is resolved and one of the lateral borders must be an exit
                            exitSets.Add(new ExitSet(1, 1, [aisle.Borders[3]]));
                            exitSets.Add(new ExitSet(1, 1, aisle.LateralBorders()));
                        }
                        else if (proximalAisleChannelTileCount == 2 || proximalAisleChannelTileCount == 3)
                        {
                            // One of the normal borders must be an exit
                            exitSets.Add(new ExitSet(1, 1, aisle.NormalBorders()));
                        }
                        else if (proximalAisleChannelTileCount >= 4)
                        {
                            // Either 1 or 2 exits
                            exitSets.Add(new ExitSet(1, 2, aisle.Borders));
                        }
                        break;

                    case 5:
                        if (proximalAisleChannelTileCount == 0)
                        {
                            // Both exits are resolved
                            exitSets.Add(new ExitSet(1, 1, [aisle.Borders[1]]));
                            exitSets.Add(new ExitSet(1, 1, [aisle.Borders[^2]]));
                        }
                        else if (proximalAisleChannelTileCount == 2 || proximalAisleChannelTileCount == 3)
                        {
                            // One of the lateral borders must be an exit
                            exitSets.Add(new ExitSet(1, 1, aisle.LateralBorders()));
                        }
                        break;

                    case 6:
                        if (proximalAisleChannelTileCount == 0)
                        {
                            // One lateral and one normal exit must be an exit
                            exitSets.Add(new ExitSet(1, 1, aisle.LateralBorders()));
                            exitSets.Add(new ExitSet(1, 1, [aisle.Borders[1], aisle.Borders[^2]]));
                        }
                        else if (proximalAisleChannelTileCount == 2 || proximalAisleChannelTileCount == 3)
                        {
                            // One of the normal borders must be an exit
                            exitSets.Add(new ExitSet(1, 1, [aisle.Borders[1], aisle.Borders[^2]]));
                        }
                        break;

                    case 7:
                        if (proximalAisleChannelTileCount == 0)
                        {
                            // Both exits are resolved to be the lateral borders
                            exitSets.Add(new ExitSet(1, 1, [aisle.Borders[0]]));
                            exitSets.Add(new ExitSet(1, 1, [aisle.Borders[^1]]));
                        }
                        else if (proximalAisleChannelTileCount == 2 || proximalAisleChannelTileCount == 3)
                        {
                            // One of the lateral borders must be an exit
                            exitSets.Add(new ExitSet(1, 1, aisle.LateralBorders()));
                        }
                        break;
                }
            }
            else
            {
                switch (aisle.ChannelTileCount)
                {
                    case 1:
                        // One of the (lateral) borders must be an exit
                        exitSets.Add(new ExitSet(1, 1, aisle.Borders));
                        break;

                    case 3:
                        if (tableau.Aisles[(aisle.Axis, aisle.Index - 1)].ChannelTileCount > 0 &&
                            tableau.Aisles[(aisle.Axis, aisle.Index + 1)].ChannelTileCount > 0)
                        {
                            // One of the (lateral) borders must be an exit
                            exitSets.Add(new ExitSet(1, 1, aisle.Borders));
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
                exitSets.Add(new ExitSet(1, 1, adjacentBorders));
            }
        }

        if (exitSets.Count == 0)
        {
            foreach (var aisle in tableau.GetMarginAisles())
            {
                switch (aisle.ChannelTileCount)
                {
                    case 1:
                        foreach (var border in aisle.NormalBorders())
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
        var possibleExits = new HashSet<Edge>();

        // Iterate through each possible border in each exit set and, for each distinct
        // border, add a dictionary entry whose key is the border and whose value is a
        // struct that references the original exit set.
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

        if (potentialExits.Count == 1)
        {
            possibleExits.Add(potentialExits.First().Value.Border);
        }
        else if (potentialExits.Count >= 2)
        {
            if (exitSets.All(exitSet => exitSet.MinExits < 2) && 
                potentialExits.Values.Any(exit => exit.ExitSets.Count == exitSets.Count))
            {
                // A single potential exit can account for all of the exit sets and no exit set must include both exits
                foreach (var exit in potentialExits.Values)
                {
                    possibleExits.Add(exit.Border);
                }
            }
            else
            {
                // Search for pairs of exits that can account for all of the exit sets
                foreach (var (exit1, exit2) in potentialExits.Values.GetAllPairs())
                {
                    if (exitSets.Except(exit1.ExitSets).Except(exit2.ExitSets).Any())
                    {
                        // Ignore cases where there must be an exit that is not in this pair of exit sets
                        continue;
                    }

                    var commonExitSets = exit1.ExitSets.Intersect(exit2.ExitSets);
                    if (commonExitSets.Any(exitSet => exitSet.MaxExits < 2))
                    {
                        // Ignore cases where the pair of potential exits are in an exit set that cannot have 2 exits.
                        continue;
                    }

                    possibleExits.Add(exit1.Border);
                    possibleExits.Add(exit2.Border);
                }

                if (possibleExits.Count == 2)
                {
                    // Both exits are resolved.
                    foreach (var border in possibleExits)
                    {
                        border.TryResolve(Resolution.Channel, notifier, ResolutionReason.ExitCount);
                    }

                    // No remaining unresolved border can be an exit.
                    foreach (var border in tableau.GetBorders())
                    {
                        border.TryResolve(Resolution.Empty, notifier, ResolutionReason.Housekeeping);
                    }

                    return;
                }

                if (potentialExits.Values.None(exit => exit.ExitSets.Count == exitSets.Count))
                {
                    // No single potential exit can account for all the identified exit sets so both exits
                    // must be in the possibleExits set and all borders not in this set can be marked as empty.
                    foreach (var border in tableau.GetBorders().Except(possibleExits))
                    {
                        border.TryResolve(Resolution.Empty, notifier, ResolutionReason.ExitCount);
                    }

                    return;
                }
            }
        }

        if (possibleExits.Count > 0)
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
                    // If there is an exit that does not overlap with the borders of the aisle,
                    // then there can be at most one exit in this aisle, so we can resolve some
                    // aisle borders for specific channel tile counts.
                    switch (aisle.ChannelTileCount)
                    {
                        case 1:
                        case 3:
                            // Any potential exit in this aisle cannot be normal to the axis of the aisle
                            foreach (var border in aisle.NormalBorders())
                            {
                                border.TryResolve(Resolution.Empty, notifier, ResolutionReason.BorderAisleCountWithSinglePotentialExit);
                            }
                            break;

                        case 2:
                            // Any potential exit in this aisle must be normal to the axis of the aisle
                            foreach (var border in aisle.LateralBorders())
                            {
                                border.TryResolve(Resolution.Empty, notifier, ResolutionReason.BorderAisleCountWithSinglePotentialExit);
                            }
                            break;

                        case 6:
                            // Central normal border cannot be an exit if there is a single exit
                            foreach (var border in aisle.NormalBorders())
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

    /// <summary>
    /// Represents a set of borders that must include 1 or 2 exits
    /// </summary>
    public readonly struct ExitSet(int minExits, int maxExits, IEnumerable<Edge> borders)
    {
        public HashSet<Edge> Borders { get; } = [.. borders];

        public int MinExits { get; } = minExits;

        public int MaxExits { get; } = maxExits;
    }
}
