using Solver.Components;
using Solver.Framework;
using static Solver.Framework.LinqExtensions;

namespace Solver.Rules;

/// <summary>
/// Identify intersecting aisle counts that mandate a non-border channel in an aisle with an unresolved channel count of 2,
/// such that we can mark other unresolved tiles in that aisle as empty.
/// </summary>
public class AisleCountIntersectionRule(Tableau tableau) : Rule(tableau)
{
    public override string Name => nameof(AisleCountIntersectionRule);

    public override IEnumerable<ComponentType> GetPertinentComponents()
    {
        yield return ComponentTypes.Tableau;
        yield return ComponentTypes.Aisle;
        yield return ComponentTypes.Tile;
    }

    public override void Invoke(IComponent component, INotifier notifier)
    {
        switch (component)
        {
            case Tableau tableau:
                foreach (var aisle in tableau.GetAisles())
                {
                    InvokeInternal(aisle, notifier);
                }
                break;

            case Aisle aisle:
                InvokeInternal(aisle, notifier);
                break;

            case Tile tile:
                foreach (var aisle in tile.Aisles)
                {
                    InvokeInternal(aisle, notifier);
                }
                break;
        }
    }

    private static void InvokeInternal(Aisle aisle, INotifier notifier)
    {
        if (aisle.UnresolvedChannelTileCount == 2)
        {
            foreach (var (tile1, tile2) in aisle.Tiles.SelectWithNext(SelectWithNextOption.Pairs))
            {
                if (tile1.IsResolved || tile2.IsResolved)
                {
                    continue;
                }

                if (tile1.HasBorder || tile2.HasBorder)
                {
                    continue;
                }

                var otherAisle = tile1.Aisles
                    .Intersect(tile2.Aisles)
                    .First(a => a.Axis != aisle.Axis);

                if (otherAisle.UnresolvedEmptyTileCount == 1)
                {
                    var possibleChannelTiles = tile1.GetAdjacentTiles(aisle.Axis)
                        .Union(tile2.GetAdjacentTiles(aisle.Axis));

                    if (possibleChannelTiles.All(t => t.Resolution != Resolution.Channel))
                    {
                        var possibleEmptyTiles = otherAisle.IsMargin
                            ? aisle.Tiles.Where(t => !t.IsResolved && !t.HasBorder)
                            : aisle.Tiles.Where(t => !t.IsResolved);

                        foreach (var tile in possibleEmptyTiles.Except(possibleChannelTiles))
                        {
                            tile.TryResolve(Resolution.Empty, notifier, ResolutionReason.AisleCountIntersection);
                        }
                    }
                }
            }
        }
    }
}
