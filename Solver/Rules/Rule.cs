using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

public abstract class Rule : IRule
{
    protected internal readonly Tableau _tableau;

    protected Rule(Tableau tableau)
    {
        _tableau = tableau;
    }

    public abstract IEnumerable<Type> GetPertinentComponents();

    public abstract void Invoke(IComponent component, INotifier notifier);
}
