using System.Collections.Generic;

namespace Combinations
{
    public interface IRoute<out T>
    {
        T Value { get; }

        IRoute<T> Previous { get; }

        IEnumerable<T> AllValues { get; }
    }
}
