using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

[RulePrioriry(QueuePriority.VeryHigh)]
public class HousekeepingRule(Tableau tableau) : Rule(tableau)
{
    public override string ToString()
    {
        return nameof(HousekeepingRule);
    }

    public override IEnumerable<Type> GetPertinentComponents()
    {
        yield return typeof(Tile);
        yield return typeof(Edge);
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
