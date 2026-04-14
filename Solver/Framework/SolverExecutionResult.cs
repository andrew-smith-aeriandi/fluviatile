namespace Solver.Framework;

public record SolverExecutionResult
{
    public SolverStatus Status { get; init; }

    public int RuleInvocationCount { get; init; }

    public int ResolutionReasonCount { get; init; }

    public int HypotheticalsCount { get; init; }

    public double Difficulty { get; init; }

    public override string ToString()
    {
        return $"{Status}; Rule Invocations: {RuleInvocationCount}, Reasons: {ResolutionReasonCount}, Hypotheticals: {HypotheticalsCount}, Difficulty: {Difficulty:0.000}";
    }
}
