using Solver.Components;
using Solver.Framework;
using System.Diagnostics;

namespace Solver.Rules;

public class HousekeepingRule : IRule
{
    private readonly Tableau _tableau;

    public HousekeepingRule(Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        _tableau = tableau;
    }

    public IEnumerable<Type> GetPertinentComponents()
    {
        yield return typeof(Tile);
        yield return typeof(Edge);
    }

    public void Invoke(IComponent component, INotifier notifier)
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

    private void InvokeInternal(Tile tile, INotifier notifier)
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

    public override string ToString()
    {
        return nameof(HousekeepingRule);
    }
}
