using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

public abstract class Rule(Tableau tableau) : IRule
{
    protected internal readonly Tableau _tableau = tableau;

    public abstract string Name { get; }

    public abstract IEnumerable<ComponentType> GetPertinentComponents();

    public abstract void Invoke(IComponent component, INotifier notifier);

    public override string ToString() => Name;
}
