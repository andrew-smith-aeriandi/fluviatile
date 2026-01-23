using System.Runtime.CompilerServices;

namespace Solver.Framework;

public static class Int32Extensions
{
    /// <summary>
    /// Determines if value is in the specified range (inclusive lower and upper bounds)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRange(this int value, int min, int max) =>
        min <= value && value <= max;

    /// <summary>
    /// Determines if value is in the specified range (inclusive lower and exclusive upper bounds)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRangeExclusiveUpper(this int value, int min, int max) =>
        min <= value && value < max;

    /// <summary>
    /// Determines if value is in the specified range (exclusive lower and inclusive upper bounds)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRangeExclusiveLower(this int value, int min, int max) =>
        min < value && value <= max;

    /// <summary>
    /// Determines if value is in the specified range (exclusive lower and upper bounds)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRangeExclusive(this int value, int min, int max) =>
        min < value && value < max;
}
