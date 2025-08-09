using System.Runtime.CompilerServices;

namespace Solver.Framework;

public static class LinqExtensions
{
    public static int IndexOf<T>(
        this IReadOnlyList<T> source,
        T element)
    {
        if (source is IList<T> list)
        {
            return list.IndexOf(element);
        }

        var comparer = EqualityComparer<T>.Default;
        for (var index = 0; index < source.Count; index++)
        {
            if (comparer.Equals(element, source[index]))
            {
                return index;
            }
        }

        return -1;
    }

    public static bool HasAtLeast<TSource>(
        this IEnumerable<TSource> source,
        int count)
    {
        return count switch
        {
            < 1 => true,
            1 => source.Any(),
            _ => source.Skip(count - 1).Any()
        };
    }

    public static void InvokeForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }

    public static bool TryGetSingle<T>(this IEnumerable<T> source, out T? result)
    {
        if (source is IList<T> list)
        {
            if (list.Count == 1)
            {
                result = list[0];
                return true;
            }

            result = default;
            return false;
        }

        using var enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
        {
            var first = enumerator.Current;
            if (!enumerator.MoveNext())
            {
                result = first;
                return true;
            }
        }

        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool None<T>(this IEnumerable<T> source)
    {
        return !source.Any();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        return !source.Any(value => predicate(value));
    }

    public static IEnumerable<UnorderedPair<T>> GetAllPairs<T>(
        this IEnumerable<T> source)
    {
        return source.SelectMany(
            (item, index) => source.Skip(index + 1),
            (item1, item2) => new UnorderedPair<T>(item1, item2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<(T, T)> SelectWithNext<T>(
        this IEnumerable<T> source,
        SelectWithNextOption option = SelectWithNextOption.Default)
    {
        return source.SelectWithNext((first, second) => (first, second!), option);
    }

    /// <summary>
    /// Projects an enumerable sequence using a function that takes the current value and the next value
    /// </summary>
    /// <typeparam name="TSource">source type</typeparam>
    /// <typeparam name="TResult">result type</typeparam>
    /// <param name="source">enumerable source sequence</param>
    /// <param name="projection">function taking current and next source values</param>
    /// <returns>Enumerable sequence of results</returns>
    public static IEnumerable<TResult> SelectWithNext<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TSource?, TResult> projection,
        SelectWithNextOption option = SelectWithNextOption.Default)
    {
        using var iterator = source.GetEnumerator();

        if (!iterator.MoveNext())
        {
            yield break;
        }

        var first = iterator.Current;
        var previous = first;

        while (iterator.MoveNext())
        {
            yield return projection(previous, iterator.Current);
            previous = iterator.Current;
        }

        switch (option)
        {
            case SelectWithNextOption.Pairs:
                break;

            case SelectWithNextOption.LoopBackToStart:
                yield return projection(previous, first);
                break;

            default:
                yield return projection(previous, default);
                break;
        }
    }

    public enum SelectWithNextOption
    {
        Default = 0,
        LoopBackToStart = 1,
        Pairs = 2
    }
}
