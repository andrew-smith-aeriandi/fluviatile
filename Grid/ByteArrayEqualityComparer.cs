using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Fluviatile.Grid
{
    public class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            if (x.Length != y.Length)
            {
                return false;
            }

            for (int index = 0; index < x.Length; index++)
            {
                if (x[index] != y[index])
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode([DisallowNull] byte[] obj)
        {
            var count = obj.Length;
            var index = count;
            var limit = count - Math.Min(count, 8);
            var hashCode = count;

            unchecked
            {
                while (--index >= limit)
                {
                    hashCode = 31 * hashCode + (int)obj[index];
                }
            }

            return hashCode;
        }
    }
}
