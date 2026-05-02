namespace Solver.Framework;

public class SolverCounts
{
    private long _solved;
    private long _unsolved;
    private long _error;
    private long _elapsedTicks;

    public long Solved => _solved;

    public long Unsolved => _unsolved;

    public long Error => _error;

    public TimeSpan SolvedTotalElapsedTime => TimeSpan.FromTicks(_elapsedTicks);

    public TimeSpan SolvedMeanElapsedTime => TimeSpan.FromTicks(_elapsedTicks / _solved);

    public SolverCounts NotifyStatus(SolverStatus status, TimeSpan elapsedTime)
    {
        switch (status)
        {
            case SolverStatus.Solved:
                Interlocked.Increment(ref _solved);
                Interlocked.Add(ref _elapsedTicks, elapsedTime.Ticks);
                break;

            case SolverStatus.Unsolved:
                Interlocked.Increment(ref _unsolved);
                break;

            case SolverStatus.Error:
                Interlocked.Increment(ref _error);
                break;
        }

        return this;
    }

    public override string ToString()
    {
        return $"Solved: {Solved} (Mean Duration: {SolvedMeanElapsedTime.TotalMilliseconds:0.000}ms), Unsolved: {Unsolved}, Error: {Error}";
    }
}
