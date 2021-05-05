using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Fluviatile.Grid
{
    public class SequenceEqualEqualityComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(int[] obj)
        {
            var value = 0;
            for (var i = 0; i < obj.Length; i++)
            {
                value = unchecked(value * 31 + obj[i]);
            }

            return value;
        }
    }
}
