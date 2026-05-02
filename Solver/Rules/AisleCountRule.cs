using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

/// <summary>
/// Identify aisles where either the unresolved channel count is zero such that any unresolved tiles must be empty, 
/// or where the unresolved empty count is zero such that any unresolved tiles must be channels. 
/// </summary>
public class AisleCountRule(Tableau tableau) : Rule(tableau)
{
    public override string Name => nameof(AisleCountRule);

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
}
