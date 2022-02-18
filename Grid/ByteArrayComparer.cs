using System;
using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public class ByteArrayComparer : IComparer<byte[]>
    {
        public int Compare(byte[] x, byte[] y)
        {
            var count = Math.Min(x.Length, y.Length);

            for (int index = 0; index < count; index++)
            {
                var result = x[index] - y[index];
                if (result != 0)
                {
                    return result;
                }
            }

            return x.Length - y.Length;
        }
    }
}
