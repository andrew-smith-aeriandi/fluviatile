using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

public class AisleResolutionPatternRule : IRule
{
    private readonly List<List<Resolution[]>> _tilePatterns;

    public AisleResolutionPatternRule(Grid grid)
    {
        ArgumentNullException.ThrowIfNull(grid);

        _tilePatterns = GetPatterns(grid);
    }

    public IEnumerable<Type> GetPertinentComponents()
    {
        yield return typeof(Tableau);
        yield return typeof(Aisle);
        yield return typeof(Tile);
    }

    public void Invoke(IComponent component, INotifier notifier)
    {
        if (_tilePatterns.Count == 0)
        {
            return;
        }

        switch (component)
        {
            case Aisle aisle:
                if (aisle.IsMargin)
                {
                    InvokeForMarginAisleInternal(aisle, notifier);
                }
                else
                {
                    InvokeForInternalAisleInternal(aisle, notifier);
                }
                break;

            case Tableau tableau:
                foreach (var aisle in tableau.GetAisles())
                {
                    if (aisle.IsMargin)
                    {
                        InvokeForMarginAisleInternal(aisle, notifier);
                    }
                    else
                    {
                        InvokeForInternalAisleInternal(aisle, notifier);
                    }
                }
                break;

            case Tile tile:
                foreach (var aisle in tile.Aisles)
                {
                    if (aisle.IsMargin)
                    {
                        InvokeForMarginAisleInternal(aisle, notifier);
                    }
                    else
                    {
                        InvokeForInternalAisleInternal(aisle, notifier);
                    }
                }
                break;
        }
    }

    private void InvokeForMarginAisleInternal(Aisle aisle, INotifier notifier)
    {
        if (aisle.UnresolvedTileCount == 0)
        {
            return;
        }

        var reason = ResolutionReason.MarginAisleResolutionPattern;
        var alternatePairs = new List<(int, int, Resolution)>();

        for (var index = 1; index < aisle.Tiles.Count; index++)
        {
            var tile1 = aisle.Tiles[index - 1];
            var tile2 = aisle.Tiles[index];

            if (tile1.IsResolved || tile2.IsResolved)
            {
                continue;
            }

            var otherAisle = tile1.Aisles
                .Intersect(tile2.Aisles)
                .First(a => a.Axis != aisle.Axis);

            if (otherAisle.UnresolvedEmptyTileCount == 1)
            {
                alternatePairs.Add((index - 1, index, Resolution.Channel));
            }
            else if (otherAisle.UnresolvedChannelTileCount == 1)
            {
                alternatePairs.Add((index - 1, index, Resolution.Empty));
            }
        }

        var patterns = _tilePatterns[aisle.ChannelTileCount];

        var mask = new Resolution[aisle.TileCount];
        foreach (var pattern in patterns)
        {
            var isMatch = aisle.Tiles
                .Select(t => t.Resolution)
                .Zip(pattern)
                .All(tuple => tuple.First == Resolution.Unknown || tuple.First == tuple.Second);

            if (isMatch && alternatePairs.Count > 0)
            {
                foreach (var (index1, index2, resolution) in alternatePairs)
                {
                    if (pattern[index1] != resolution && pattern[index2] != resolution)
                    {
                        reason = ResolutionReason.MarginAisleResolutionPatternConstrainedByAisleCountIntersection;
                        isMatch = false;
                        break;
                    }
                }
            }

            if (isMatch)
            {
                for (var i = 0; i < pattern.Length; i++)
                {
                    mask[i] |= pattern[i];
                }
            }
        }

        for (var i = 0; i < mask.Length; i++)
        {
            var tile = aisle.Tiles[i];
            if (!tile.IsResolved)
            {
                switch (mask[i])
                {
                    case Resolution.Channel:
                        tile.TryResolve(Resolution.Channel, notifier, reason);
                        break;

                    case Resolution.Empty:
                        tile.TryResolve(Resolution.Empty, notifier, reason);
                        break;
                }
            }
        }
    }

    private void InvokeForInternalAisleInternal(Aisle aisle, INotifier notifier)
    {
        if (aisle.UnresolvedTileCount == 0)
        {
            return;
        }

        if (aisle.UnresolvedTileCount == 1)
        {
            if (aisle.ResolvedTileCount == 0)
            {
                foreach (var tile in aisle.Tiles.Where(t => !t.HasBorder))
                {
                    tile.TryResolve(Resolution.Empty, notifier, ResolutionReason.AisleCountWithSingleExit);
                }
            }
            else
            {
                // TODO: this could be improved
                var tilesToResolve = new HashSet<Tile>(aisle.Tiles.Where(t => !t.IsResolved && !t.HasBorder));
                foreach (var tile in aisle.Tiles)
                {
                    if (tile.Resolution == Resolution.Channel)
                    {
                        tilesToResolve.ExceptWith(tile.GetPotentiallyLinkableTiles(aisle.Axis));
                    }
                }

                if (tilesToResolve.Count > 0)
                {
                    tilesToResolve.TryResolve(Resolution.Empty, notifier, ResolutionReason.InternalAisleChannelAdjacency);
                }
            }
        }
    }

    private static List<List<Resolution[]>> GetPatterns(Grid grid)
    {
        if (grid.Size != 3)
        {
            // TODO: Implement for other sizes
            return [];
        }

        const Resolution C = Resolution.Channel;
        const Resolution E = Resolution.Empty;

        var data = new List<List<Resolution[]>>(8);

        // 0
        data.Add(
        [
            [E, E, E, E, E, E, E]
        ]);

        // 1
        data.Add(
        [
            [C, E, E, E, E, E, E],
            [E, E, E, E, E, E, C]
        ]);

        // 2
        data.Add(
        [
            [C, E, E, E, E, E, C],
            [C, C, E, E, E, E, E],
            [E, C, C, E, E, E, E],
            [E, E, C, C, E, E, E],
            [E, E, E, C, C, E, E],
            [E, E, E, E, C, C, E],
            [E, E, E, E, E, C, C]
        ]);

        // 3
        data.Add(
        [
            [C, C, C, E, E, E, E],
            [E, E, C, C, C, E, E],
            [E, E, E, E, C, C, C],
            [C, E, C, C, E, E, E],
            [C, E, E, C, C, E, E],
            [C, E, E, E, C, C, E],
            [C, E, E, E, E, C, C],
            [C, C, E, E, E, E, C],
            [E, C, C, E, E, E, C],
            [E, E, C, C, E, E, C],
            [E, E, E, C, C, E, C]
        ]);

        // 4
        data.Add(
        [
            [C, C, C, C, E, E, E],
            [E, C, C, C, C, E, E],
            [E, E, C, C, C, C, E],
            [E, E, E, C, C, C, C],
            [C, C, C, E, E, E, C],
            [E, E, C, C, C, E, C],
            [C, E, C, C, C, E, E],
            [C, E, E, E, C, C, C],
            [C, C, E, C, C, E, E],
            [C, C, E, E, C, C, E],
            [C, C, E, E, E, C, C],
            [E, C, C, E, C, C, E],
            [E, C, C, E, E, C, C],
            [E, E, C, C, E, C, C]
        ]);

        // 5
        data.Add(
        [
            [C, C, C, C, C, E, E],
            [C, C, C, C, E, E, C],
            [C, C, C, E, E, C, C],
            [C, C, E, E, C, C, C],
            [C, E, E, C, C, C, C],
            [E, E, C, C, C, C, C],
            [E, C, C, C, C, E, C],
            [C, E, C, C, C, C, E],
            [C, E, C, C, C, E, C],
            [C, C, C, E, C, C, E],
            [E, C, C, E, C, C, C]
        ]);

        // 6
        data.Add(
        [
            [C, C, C, C, C, C, E],
            [C, C, C, C, C, E, C],
            [C, C, C, C, E, C, C],
            [C, C, C, E, C, C, C],
            [C, C, E, C, C, C, C],
            [C, E, C, C, C, C, C],
            [E, C, C, C, C, C, C]
        ]);

        // 7
        data.Add(
        [
            [C, C, C, C, C, C, C]
        ]);

        return data;
    }

    public override string ToString()
    {
        return nameof(AisleResolutionPatternRule);
    }
}
