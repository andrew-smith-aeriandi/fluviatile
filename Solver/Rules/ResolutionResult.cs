using Solver.Components;

namespace Solver.Rules;

public record ResolutionResult(
    IComponent Component,
    ResolutionReason Reason);
