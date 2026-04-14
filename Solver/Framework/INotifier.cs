using Solver.Components;

namespace Solver.Framework;

public interface INotifier
{
    void NotifyResolution(IComponent component, ResolutionReason reason = ResolutionReason.Unspecified);
}
