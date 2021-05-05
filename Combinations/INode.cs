using System.Collections.Generic;

namespace Combinations
{
    public interface INode<out T>
    {
        T Value { get; }

        IEnumerable<INode<T>> Links { get; }
    }
}
