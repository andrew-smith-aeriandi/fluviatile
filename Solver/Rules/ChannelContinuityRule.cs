using Solver.Components;
using Solver.Framework;
using System.Diagnostics;

namespace Solver.Rules;

public class ChannelContinuityRule : IRule
{
    public IEnumerable<Type> GetPertinentComponents()
    {
        yield return typeof(Thalweg);
    }

    public void Invoke(IComponent component, INotifier notifier)
    {
        switch (component)
        {
            case Thalweg thalweg:
                InvokeInternal(thalweg, notifier);
                break;
        }
    }

    private void InvokeInternal(Thalweg thalweg, INotifier notifier)
    {
        var segmentsWithOneTermination = new List<(Tile Tile, int TileCount)>();

        foreach (var segment in thalweg.Segments)
        {
            switch (segment.First, segment.Last)
            {
                case (Tile tile1, Tile tile2):
                    if (tile1.TryGetCommonEdge(tile2, out var edge) && edge is not null)
                    {
                        // Cannot be a closed loop
                        edge.TryResolve(Resolution.Empty, notifier, ResolutionReason.NoClosedLoop);
                    }
                    else if (Math.Abs(segment.Rotation) == 5)
                    {
                        // Cannot be a closed loop
                        foreach (var tile in tile1.GetPotentiallyLinkableTiles().Intersect(tile2.GetPotentiallyLinkableTiles()))
                        {
                            tile.TryResolve(Resolution.Empty, notifier, ResolutionReason.NoClosedLoop);
                        }
                    }
                    break;

                case (Termination _, Tile tile2):
                    segmentsWithOneTermination.Add((tile2, segment.TileCount));
                    break;

                case (Tile tile1, Termination _):
                    segmentsWithOneTermination.Add((tile1, segment.TileCount));
                    break;
            }

            foreach (var (tile, tileCount) in segmentsWithOneTermination)
            {
                if (tileCount < thalweg.TileCount)
                {
                    // Cannot terminate until all channel tiles are linked
                    foreach (var border in tile.Edges.Where(e => e.IsBorder))
                    {
                        border.TryResolve(Resolution.Empty, notifier, ResolutionReason.SingleChannel);
                    }
                }
            }

            if (segmentsWithOneTermination.Count == 2)
            {
                var commonEdge = segmentsWithOneTermination[0].Tile.Edges
                    .Intersect(segmentsWithOneTermination[1].Tile.Edges)
                    .SingleOrDefault();

                if (commonEdge is not null)
                {
                    var combinedTileCount = segmentsWithOneTermination.Sum(t => t.TileCount);
                    if (combinedTileCount < thalweg.TileCount)
                    {
                        // Cannot link terminating segments until all channel tiles are included
                        commonEdge.TryResolve(Resolution.Empty, notifier, ResolutionReason.SingleChannel);
                    }
                    else if (combinedTileCount == thalweg.TileCount)
                    {
                        // Link segments to complete channel
                        commonEdge.TryResolve(Resolution.Channel, notifier, ResolutionReason.SingleChannel);
                        thalweg.TryLink(commonEdge, notifier);
                    }
                    else
                    {
                        throw new UnreachableException($"Combined channel segment tile count cannot exceed {thalweg.TileCount}.");
                    }
                }
            }
        }
    }

    public override string ToString()
    {
        return nameof(ChannelContinuityRule);
    }
}
