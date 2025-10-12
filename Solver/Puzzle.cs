namespace Solver;

public record Puzzle
{
    public Puzzle(IEnumerable<int> channelCounts, bool solved, string comment)
    {
        ChannelCounts = [.. channelCounts];
        Solved = solved;
        Comment = comment;
    }

    public int[] ChannelCounts { get; }

    public bool Solved { get; }

    public string Comment { get; }
}
