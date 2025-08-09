using Solver.Framework;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Solver.Components;

public static class AisleExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Axis, int) GetDefaultKey(this Aisle aisle) => (aisle.Axis, aisle.Index);

    public static Func<Tile, int> SortOrderKeySelector(this Aisle aisle) => aisle.Axis switch
    {
        Axis.X => tile => tile.Coordinates.Y,
        Axis.Y => tile => tile.Coordinates.Z,
        Axis.Z => tile => tile.Coordinates.X,
        _ => throw new UnreachableException($"Usupported axis: {aisle.Axis}")
    };

    public static bool IsFirstOrLastTile(this Aisle aisle, Tile tile)
    {
        return tile == aisle.Tiles[0] || tile == aisle.Tiles[^1];
    }

    public static (int Exits, int Unresolved) ExitCount(this Aisle aisle, Tableau tableau)
    {
        var exitCount = 0;
        var unresolvedCount = 0;

        if (aisle.Index == 0 || aisle.Index == tableau.Grid.AisleCountPerAxis - 1)
        {
            var borderEdges = aisle.Tiles
                .SelectMany(t => t.Edges)
                .Where(e => e.IsBorder && e.NormalAxis == aisle.Axis);

            foreach (var edge in borderEdges)
            {
                switch (edge.Resolution)
                {
                    case Resolution.Channel:
                        exitCount += 1;
                        break;

                    case Resolution.Unknown:
                        unresolvedCount += 1;
                        break;
                }
            }
        }

        return (exitCount, unresolvedCount);
    }
}

