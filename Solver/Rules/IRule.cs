using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

public interface IRule
{
    string Name { get; }

    IEnumerable<ComponentType> GetPertinentComponents();

    void Invoke(IComponent component, INotifier notifier);
}
