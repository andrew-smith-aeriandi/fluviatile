using Solver.Framework;
using System.Diagnostics;

namespace Solver.Components;

public static class ComponentExtensions
{
    public static ComponentType GetComponentType(this IComponent component)
    {
        return component switch
        {
            Tableau _ => ComponentTypes.Tableau,
            Aisle _ => ComponentTypes.Aisle,
            Tile _ => ComponentTypes.Tile,
            Edge _ => ComponentTypes.Edge,
            Thalweg _ => ComponentTypes.Thalweg,
            Thalweg.Segment _ => ComponentTypes.ThalwegSegment,
            Termination _ => ComponentTypes.Termination,
            _ => throw new UnreachableException()
        };
    }

    public static ResolutionCounts GetCounts(this IEnumerable<IResolvableComponent> resolvableComponents)
    {
        return resolvableComponents.Aggregate(
            new ResolutionCounts(0, 0, 0),
            (counts, component) =>
            {
                switch (component.Resolution)
                {
                    case Resolution.Unknown:
                        counts.Unknown += 1;
                        break;

                    case Resolution.Channel:
                        counts.Channel += 1;
                        break;

                    case Resolution.Empty:
                        counts.Empty += 1;
                        break;
                }

                return counts;
            });
    }

    public static bool TryResolve(
        this IEnumerable<IResolvableComponent> resolvableComponents,
        Resolution resolution,
        INotifier notifier,
        ResolutionReason reason = ResolutionReason.Unspecified)
    {
        var resolutionCount = 0;

        foreach (var resolvableComponent in resolvableComponents)
        {
            if (resolvableComponent.TryResolve(resolution, notifier, reason))
            {
                resolutionCount += 1;
            }
        }

        return resolutionCount > 0;
    }
}

