using Solver.Components;
using Solver.Rules;

public record ResolutionResult(
    IComponent Component,
    ResolutionReason Reason);
