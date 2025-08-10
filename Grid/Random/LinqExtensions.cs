using System;
using System.Collections.Generic;

namespace Fluviatile.Grid.Random
{
    public static class LinqExtensions
    {
        public static T RandomElement<T>(
            this IEnumerable<T> source,
            IRandom random)
        {
            ArgumentNullException.ThrowIfNull(random);

            var iterator = source.GetEnumerator();
            var current = default(T);
            var count = 0;

            while (iterator.MoveNext())
            {
                count++;
                if (random.Try(1, count))
                {
                    current = iterator.Current;
                }
            }

            return current;
        }
    }
}
