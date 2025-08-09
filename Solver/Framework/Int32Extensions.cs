using System.Runtime.CompilerServices;

namespace Solver.Framework;

public static class Int32Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRange(this int value, int min, int max) =>
        min <= value && value <= max;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRangeExclusiveUpper(this int value, int min, int max) =>
        min <= value && value < max;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRangeExclusiveLower(this int value, int min, int max) =>
        min < value && value <= max;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRangeExclusive(this int value, int min, int max) =>
        min < value && value < max;
}
