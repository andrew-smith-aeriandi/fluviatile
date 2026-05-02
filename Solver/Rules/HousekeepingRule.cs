using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

/// <summary>
/// Simple housekeeping rules 
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>An empty tile's edges must all be empty.</item>
/// <item>An internal edge that has a channel must have a channel tile on either side that forms part of a thalweg segment.</item>
/// <item>A border edge that has a channel tile must be an exit</item>
/// </list>
/// </remarks>
public class HousekeepingRule(Tableau tableau) : Rule(tableau)
{
    public override string Name => nameof(HousekeepingRule);

    public override IEnumerable<ComponentType> GetPertinentComponents()
    {
        yield return ComponentTypes.Tile;
        yield return ComponentTypes.Edge;
    }

    public override void Invoke(IComponent component, INotifier notifier)
    {
        switch (component)
        {
            case Tile tile:
                InvokeInternal(tile, notifier);
                break;

            case Edge edge:
                InvokeInternal(edge, notifier);
                break;
        }
    }

    private static void InvokeInternal(Tile tile, INotifier notifier)
    {
        if (tile.Resolution == Resolution.Empty)
        {
            tile.Edges.TryResolve(Resolution.Empty, notifier, ResolutionReason.Housekeeping);
        }
    }

    private void InvokeInternal(Edge edge, INotifier notifier)
    {
        if (edge.Resolution == Resolution.Channel)
        {
            foreach (var tile in edge.Tiles)
            {
                if (tile.Resolution == Resolution.Empty)
                {
                    throw new InvalidOperationException($"Unexpected tile resolution: {tile}");
                }

                tile.TryResolve(Resolution.Channel, notifier, ResolutionReason.Housekeeping);
            }

            _tableau.Thalweg.TryLink(edge, notifier, ResolutionReason.Housekeeping);
        }
    }
}
