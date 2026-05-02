namespace Solver.Framework;

public interface ICroupier
{
    IList<T> Shuffle<T>(IEnumerable<T> input);

    IList<T> Transpose<T>(IEnumerable<T> input, int count = 1);
}
