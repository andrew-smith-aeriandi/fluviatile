using System.Collections.Generic;
using System.Linq;

namespace Fluviatile.Grid
{
    public class ByteSequenceEqualEqualityComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
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
