using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

public class AisleCountRule : IRule
{
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
            case Tableau tableau:
                foreach (var (_, aisle) in tableau.Aisles)
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
        if (aisle.UnresolvedChannelTileCount == 0 && aisle.UnresolvedEmptyTileCount > 0)
        {
            // Any unresolved tiles must be empty
            aisle.Tiles.TryResolve(Resolution.Empty, notifier, ResolutionReason.AisleCount);
        }
        else if (aisle.UnresolvedEmptyTileCount == 0 && aisle.UnresolvedChannelTileCount > 0)
        {
            // Any unresolved tiles must be channels
            aisle.Tiles.TryResolve(Resolution.Channel, notifier, ResolutionReason.AisleCount);
        }
    }

    public override string ToString()
    {
        return nameof(AisleCountRule);
    }
}
