using Solver.Components;
using Solver.Rules;

namespace Solver.Framework;

public interface INotifier
{
    void NotifyResolution(IComponent component, ResolutionReason reason = ResolutionReason.Unspecified);
}
