using System.Diagnostics.CodeAnalysis;

namespace Solver.Framework;

public static class SolverResultsExtensions
{
    public static bool TryGetUniqueSolution(
        this SolverResults results,
        [MaybeNullWhen(false)] out SolverExecutionResult result)
    {
        if (results.Status == SolverStatus.Solved && results.ExecutionResults.Count == 1)
        {
            result = results.ExecutionResults[0];
            return true;
        }

        result = null;
        return false;
    }
}
