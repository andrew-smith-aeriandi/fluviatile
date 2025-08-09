using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

public interface IRule
{
    IEnumerable<Type> GetPertinentComponents();

    void Invoke(IComponent component, INotifier notifier);
}
