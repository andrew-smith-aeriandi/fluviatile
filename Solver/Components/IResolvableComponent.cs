using Solver.Framework;

namespace Solver.Components;

public interface IResolvableComponent : IComponent
{
    Resolution Resolution { get; }

    bool IsResolved { get; }

    bool TryResolve(
        Resolution resolution,
        INotifier notifier,
        ResolutionReason reason = ResolutionReason.Unspecified);
}
