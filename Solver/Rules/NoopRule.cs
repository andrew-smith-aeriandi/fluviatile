using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

public class NoopRule(Tableau tableau) : Rule(tableau)
{
    public override string ToString()
    {
        return nameof(NoopRule);
    }

    public override IEnumerable<Type> GetPertinentComponents()
    {
        yield break;
    }

    public override void Invoke(IComponent component, INotifier notifier)
    {
    }
}
