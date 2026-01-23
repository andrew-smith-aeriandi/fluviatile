using Solver.Framework;

namespace Solver;

public record Puzzle
{
    public Puzzle(IEnumerable<int> channelCounts, SolverStatus solverResult, string comment)
    {
        ChannelCounts = [.. channelCounts];
        SolverResult = solverResult;
        Comment = comment;
    }

    public int[] ChannelCounts { get; }

    public SolverStatus SolverResult { get; }

    public string Comment { get; }

    public override string ToString()
    {
        return $"{SolverResult}, {Comment}";
    }
}
