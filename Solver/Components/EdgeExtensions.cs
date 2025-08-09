using Solver.Framework;
using System.Runtime.CompilerServices;

namespace Solver.Components;

public static class EdgeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnorderedPair<Coordinates> GetDefaultKey(this Edge edge) => edge.Vertices;
}
