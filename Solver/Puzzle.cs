using Solver.Framework;

namespace Solver;

public record Puzzle
{
    public Puzzle(IEnumerable<int> channelCounts, SolverResult solverResult, string comment)
    {
        ChannelCounts = [.. channelCounts];
        SolverResult = solverResult;
        Comment = comment;
    }

    public int[] ChannelCounts { get; }

    public SolverResult SolverResult { get; }

    public string Comment { get; }

    public override string ToString()
    {
        return $"{SolverResult}, {Comment}";
    }
}
