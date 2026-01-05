using Solver.Framework;
using System.Runtime.CompilerServices;

namespace Solver.Components;

public static class EdgeExtensions
{
    public static Edge Clone(this Edge source, SolverGrid grid)
    {
        ArgumentNullException.ThrowIfNull(source);

        var (vertex1, vertex2) = source.Vertices;
        return new Edge(vertex1, vertex2, grid, source.Resolution);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnorderedPair<Coordinates> GetDefaultKey(this Edge edge) => edge.Vertices;
}
