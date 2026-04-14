using Solver.Components;
using Solver.Framework;
using System.Diagnostics;

namespace Solver.Rules;

[RulePrioriry(QueuePriority.High)]
public class MeanderRule(Tableau tableau) : Rule(tableau)
{
    public override string ToString()
    {
        return nameof(MeanderRule);
    }

    public override IEnumerable<Type> GetPertinentComponents()
    {
        yield return typeof(Tableau);
        yield return typeof(Tile);
        yield return typeof(Edge);
    }

    public override void Invoke(IComponent component, INotifier notifier)
    {
        switch (component)
        {
            case Tableau tableau:
                foreach (var edge in tableau.GetEdges())
                {
                    InvokeInternal(edge, notifier);
                }
                break;

            case Tile tile:
                foreach (var edge in tile.Edges)
                {
                    InvokeInternal(edge, notifier);
                }
                break;

            case Edge edge:
                InvokeInternal(edge, notifier);
                break;
        }
    }

    private static void InvokeInternal(Edge edge, INotifier notifier)
    {
        if (edge.Resolution == Resolution.Empty)
        {
            foreach (var tile in edge.Tiles.Where(t => t.Resolution == Resolution.Channel))
            {
                foreach (var otherEdge in tile.GetEdges(edge.NormalAxis))
                {
                    if (otherEdge.Resolution == Resolution.Empty)
                    {
                        throw new UnreachableException($"Unexpected edge resolution: {otherEdge}");
                    }

                    otherEdge.TryResolve(Resolution.Channel, notifier, ResolutionReason.MeanderRule);
                }
            }
        }
    }
}
