namespace Solver.Framework;

public static class IListExtensions
{
    public static IEnumerable<T> GetRange<T>(this IEnumerable<T> source, Range range)
    {
        return source.Skip(range.Start.Value - 1).Take(range.End.Value - range.Start.Value);
    }
}
