using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

public class NoopRule(Tableau tableau) : Rule(tableau)
{
    public override string Name => nameof(NoopRule);

    public override IEnumerable<ComponentType> GetPertinentComponents()
    {
        yield break;
    }

    public override void Invoke(IComponent component, INotifier notifier)
    {
    }
}
