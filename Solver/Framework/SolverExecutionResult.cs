namespace Solver.Framework;

public record SolverExecutionResult
{
    /// <summary>
    /// Indicates whether the tableau was solved, or if there was a logical error indicating an invalid solution (or bug in the code)
    /// </summary>
    public SolverStatus Status { get; init; }

    /// <summary>
    /// Time taken while attempting to solve tableau
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>
    /// Number of rules invoked while solving the tableau
    /// </summary>
    public int RuleInvocationCount { get; init; }

    /// <summary>
    /// Number of different resolution reasons recorded while solving the tableau
    /// </summary>
    public int ResolutionReasonCount { get; init; }

    /// <summary>
    /// Number of hypotheticals invoked while solving the tableau
    /// </summary>
    public int HypotheticalsCount { get; init; }

    /// <summary>
    /// A heuristic measure of the difficulty in solving the tableau
    /// </summary>
    public double Difficulty { get; init; }

    public override string ToString()
    {
        return $"{Status}; Time: {ElapsedTime.TotalMilliseconds:0.000}ms, Rule Invocations: {RuleInvocationCount}, Reasons: {ResolutionReasonCount}, Hypotheticals: {HypotheticalsCount}, Difficulty: {Difficulty:0.000}";
    }
}
