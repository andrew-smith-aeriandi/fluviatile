using Solver.Components;
using Solver.Framework;
using System.Diagnostics;

namespace Solver.Rules;

public class TileEdgeRule : IRule
{
    public override string ToString()
    {
        return nameof(TileEdgeRule);
    }

    public IEnumerable<Type> GetPertinentComponents()
    {
        yield return typeof(Tableau);
        yield return typeof(Tile);
        yield return typeof(Edge);
    }

    public void Invoke(IComponent component, INotifier notifier)
    {
        switch (component)
        {
            case Tableau tableau:
                foreach (var tile in tableau.Tiles.Values)
                {
                    InvokeInternal(tile, notifier);
                }
                break;

            case Tile tile:
                InvokeInternal(tile, notifier);
                break;

            case Edge edge:
                if (edge.TileMinus is not null)
                {
                    InvokeInternal(edge.TileMinus, notifier);
                }

                if (edge.TilePlus is not null)
                {
                    InvokeInternal(edge.TilePlus, notifier);
                }
                break;
        }
    }

    private static void InvokeInternal(Tile tile, INotifier notifier)
    {
        var counts = tile.Edges.GetCounts();

        switch (counts)
        {
            case (2, 1, 0):
                tile.TryResolve(Resolution.Channel, notifier, ResolutionReason.TileEdgesResolution);
                break;

            case (0, 3, 0):
                tile.TryResolve(Resolution.Empty, notifier, ResolutionReason.TileEdgesResolution);
                break;

            case (2, 0, 1):
                tile.TryResolve(Resolution.Channel, notifier, ResolutionReason.TileEdgesResolution);
                tile.Edges.TryResolve(Resolution.Empty, notifier, ResolutionReason.TileEdgesResolution);
                break;

            case (1, 1, 1):
                tile.TryResolve(Resolution.Channel, notifier, ResolutionReason.TileEdgesResolution);
                tile.Edges.TryResolve(Resolution.Channel, notifier, ResolutionReason.TileEdgesResolution);
                break;

            case (0, 2, 1):
                tile.TryResolve(Resolution.Empty, notifier, ResolutionReason.TileEdgesResolution);
                tile.Edges.TryResolve(Resolution.Empty, notifier, ResolutionReason.TileEdgesResolution);
                break;

            case (1, 0, 2):
                tile.TryResolve(Resolution.Channel, notifier, ResolutionReason.TileEdgesResolution);
                break;

            case (0, 1, 2):
                if (tile.Resolution == Resolution.Empty)
                {
                    // This should be picked up by housekeeping rule, but is included here for completeness
                    tile.Edges.TryResolve(Resolution.Empty, notifier, ResolutionReason.Housekeeping);
                }
                else if (tile.Resolution == Resolution.Channel)
                {
                    // This should be picked up by Meander rule, but is included here for completeness
                    foreach (var edge in tile.Edges)
                    {
                        edge.TryResolve(Resolution.Channel, notifier, ResolutionReason.MeanderRule);
                    }
                }
                else if (tile.Resolution == Resolution.Unknown)
                {
                    // Register components to resolve before resolving them to avoid mutating state
                    var componentsToResolve = new Dictionary<IResolvableComponent, Resolution>();

                    foreach (var edge in tile.Edges)
                    {
                        var aisle = tile.GetAisle(edge.NormalAxis);
                        var unresolvedAdjacentTiles = tile.GetPotentiallyLinkableTiles(aisle.Axis)
                            .Where(t => !t.IsResolved)
                            .ToArray();

                        if (aisle.UnresolvedChannelTileCount.IsInRange(1, unresolvedAdjacentTiles.Length))
                        {
                            componentsToResolve.TryAdd(tile, Resolution.Empty);
                        }

                        if (aisle.UnresolvedEmptyTileCount <= 1)
                        {
                            foreach (var unresolvedAdjacentTile in unresolvedAdjacentTiles)
                            {
                                componentsToResolve.TryAdd(unresolvedAdjacentTile, Resolution.Channel);
                            }
                        }
                    }

                    foreach (var (component, resolution) in componentsToResolve)
                    {
                        component.TryResolve(resolution, notifier, ResolutionReason.HypotheticalMeanderRuleConstrainedByAisleCount);
                    }
                }
                break;

            case (0, 0, 3):
                if (tile.Resolution == Resolution.Empty)
                {
                    // This should be picked up by housekeeping rule, but it included here for completeness
                    tile.Edges.TryResolve(Resolution.Empty, notifier, ResolutionReason.Housekeeping);
                }
                else if (tile.Resolution == Resolution.Channel)
                {
                    if (!tile.HasBorder && tile.GetPotentiallyLinkableTiles().All(t => !t.IsResolved))
                    {
                        var aisles = tile.Aisles.Where(a => a.UnresolvedChannelTileCount >= 2);
                        if (aisles.TryGetSingle(out var aisle))
                        {
                            tile.GetEdges(aisle!.Axis)
                                .TryResolve(Resolution.Channel, notifier, ResolutionReason.MeanderRuleConstrainedByAisleCounts);
                        }
                    }
                }
                break;

            default:
                throw new UnreachableException($"Invalid counts for {tile}: ({counts})");
        }
    }
}
