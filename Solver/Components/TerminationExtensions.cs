using Solver.Framework;
using System.Runtime.CompilerServices;

namespace Solver.Components;

public static class TerminationExtensions
{
    public static Termination Clone(this Termination source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Termination(source.Coordinates);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coordinates GetDefaultKey(this Termination termination) => termination.Coordinates;
}
