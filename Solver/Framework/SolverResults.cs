namespace Solver.Framework;

public record SolverResults
{
    public SolverStatus Status { get; init; }

    public required IReadOnlyList<SolverExecutionResult> ExecutionResults { get; init; }

    public override string ToString()
    {
        return $"{Status}; Solver Execution Count: {ExecutionResults.Count}";
    }
}
