namespace Solver.Framework;

public static class Maths
{
    /// <summary>
    /// Implements the modulus which is similar to the .NET % operator,
    /// but works correctly for negative numbers such that the returned 
    /// value is always non-negtive for positive divisors and always
    /// negative for negative divisors.
    /// </summary>
    /// <param name="value">dividend</param>
    /// <param name="n">divisor</param>
    /// <returns>modulus in the range [0,n)</returns>
    public static int Mod(int value, int n)
    {
        var m = value % n;
        return (m < 0 && n > 0) || (m > 0 && n < 0) ? m + n : m;
    }
}
