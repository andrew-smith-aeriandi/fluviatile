using System.Collections.Generic;

namespace Combinations;

public class Route<T> : IRoute<T>
{
    public Route(T value, IRoute<T> previous)
    {
        Value = value;
        Previous = previous;
    }

    public T Value { get; }

    public IRoute<T> Previous { get; }

    public IEnumerable<T> AllValues
    {
        get
        {
            yield return Value;

            for (var iterator = Previous; iterator is not null; iterator = iterator.Previous)
            {
                yield return iterator.Value;
            }
        }
    }

    public override string ToString()
    {
        return string.Join(";", AllValues);
    }
}
