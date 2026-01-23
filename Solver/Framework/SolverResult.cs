namespace Solver.Framework;

public record SolverResult
{
    public SolverStatus Status { get; init; }
    public int SolvedCount { get; init; }
    public int HypotheticalsCount { get; init; }
    public double Difficulty { get; init; }

    public override string ToString()
    {
        return $"{Status}; Difficulty: {Difficulty: 0.000}; Solved Count: {SolvedCount}; Hypotheticals: {HypotheticalsCount}";
    }
}
