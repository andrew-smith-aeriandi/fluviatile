namespace Solver.Framework;

public static class IListExtensions
{
    public static IEnumerable<T> GetRange<T>(this ICollection<T> source, Range range)
    {
        var (offset, count) = range.GetOffsetAndLength(source.Count);
        return source.Skip(offset).Take(count);
    }

    public static IEnumerable<T> GetRange<T>(this IReadOnlyCollection<T> source, Range range)
    {
        var (offset, count) = range.GetOffsetAndLength(source.Count);
        return source.Skip(offset).Take(count);
    }
}
