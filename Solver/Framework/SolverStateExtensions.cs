namespace Solver.Framework;

public static class SolverStateExtensions
{
    public static SolverExecutionResult ToSolverExecutionResult(this SolverState state)
    {
        var difficulty = state.GetDifficulty();

        return new SolverExecutionResult
        {
            Status = state.Status,
            ElapsedTime = state.ElapsedTime,
            RuleInvocationCount = state.RuleInvocationCount,
            ResolutionReasonCount = state.ResolutionReasonCount,
            HypotheticalsCount = state.HypotheticalComponentsCount,
            Difficulty = difficulty
        };
    }

    public static double GetDifficulty(this SolverState state)
    {
        var reasons = state.ResolutionResults
            .GroupBy(result => result.Reason)
            .ToDictionary(group => group.Key, group => group.Count());

        return reasons.Sum(reason => reason.Key.GetResolutionDifficulty() * reason.Value) / state.ResolutionResults.Count;
    }
}
