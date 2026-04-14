using Solver.Components;

namespace Solver.Framework;

public record ResolutionResult(
    IComponent Component,
    ResolutionReason Reason);
