namespace Solver.Framework;

public class SolverCounts
{
    private int _solved;
    private int _unsolved;
    private int _error;

    public int Solved => _solved;

    public int Unsolved => _unsolved;

    public int Error => _error;

    public SolverCounts NotifyStatus(SolverStatus status)
    {
        switch (status)
        {
            case SolverStatus.Solved:
                Interlocked.Increment(ref _solved);
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
        return $"Solved: {Solved}, Unsolved: {Unsolved}, Error: {Error}";
    }
}
