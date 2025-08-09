using Solver.Components;
using Solver.Framework;
using static Solver.Framework.LinqExtensions;

namespace Solver.Rules;

public class AisleCountIntersectionRule : IRule
{
    private readonly Grid _grid;

    public AisleCountIntersectionRule(Grid grid)
    {
        ArgumentNullException.ThrowIfNull(grid);

        _grid = grid;
    }

    public IEnumerable<Type> GetPertinentComponents()
    {
        yield return typeof(Tableau);
        yield return typeof(Aisle);
        yield return typeof(Tile);
    }

    public void Invoke(IComponent component, INotifier notifier)
    {
        switch (component)
        {
            case Aisle aisle:
                InvokeInternal(aisle, notifier);
                break;

            case Tableau tableau:
                foreach (var aisle in tableau.GetAisles())
                {
                    InvokeInternal(aisle, notifier);
                }
                break;

            case Tile tile:
                foreach (var aisle in tile.Aisles)
                {
                    InvokeInternal(aisle, notifier);
                }
                break;
        }
    }

    private void InvokeInternal(Aisle aisle, INotifier notifier)
    {
        //if (aisle.IsMargin)
        //{
        //    return;
        //}

        if (aisle.UnresolvedChannelTileCount == 2)
        {
            foreach (var (tile1, tile2) in aisle.Tiles.SelectWithNext(SelectWithNextOption.Pairs))
            {
                if (tile1.IsResolved || tile2.IsResolved)
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

    public override string ToString()
    {
        return nameof(AisleCountIntersectionRule);
    }
}
