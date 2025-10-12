using Solver.Components;
using Solver.Framework;
using System.Diagnostics;

namespace Solver.Rules;

public class ChannelContinuityRule : IRule
{
    public IEnumerable<Type> GetPertinentComponents()
    {
        yield return typeof(Tableau);
        yield return typeof(Thalweg);
        yield return typeof(Thalweg.Segment);
    }

    public void Invoke(IComponent component, INotifier notifier)
    {
        switch (component)
        {
            case Tableau tableau:
                InvokeInternal(tableau.Thalweg, notifier);
                break;

            case Thalweg thalweg:
                InvokeInternal(thalweg, notifier);
                break;

            case Thalweg.Segment segment:
                InvokeInternal(segment, notifier);
                break;
        }
    }

    private void InvokeInternal(Thalweg.Segment segment, INotifier notifier)
    {
        if (segment.First is Tile first && segment.Last is Tile last && first != last)
        {
            if (first.TryGetCommonEdge(last, out var edge) && edge is not null)
            {
                // Cannot be a closed loop
                edge.TryResolve(Resolution.Empty, notifier, ResolutionReason.NoClosedLoop);
            }
            else if (Math.Abs(segment.Rotation) == 5)
            {
                // Cannot be a closed loop
                foreach (var tile in first.GetPotentiallyLinkableTiles().Intersect(last.GetPotentiallyLinkableTiles()))
                {
                    tile.TryResolve(Resolution.Empty, notifier, ResolutionReason.NoClosedLoop);
                }
            }
        }
    }

    private void InvokeInternal(Thalweg thalweg, INotifier notifier)
    {
        if (thalweg.Segments.Count == 1)
        {
            var segment = thalweg.Segments[0];
            if (segment.TileCount == thalweg.TileCount)
            {
                // Add any missing terminations since all channel tiles have been accounted for
                if (segment.First is Tile tile1 && tile1.Edges.SingleOrDefault(e => e.IsBorder) is Edge edge1)
                {
                    edge1.TryResolve(Resolution.Channel, notifier, ResolutionReason.ThalwegChannelCount);
                }

                if (segment.Last is Tile tile2 && tile2.Edges.SingleOrDefault(e => e.IsBorder) is Edge edge2)
                {
                    edge2.TryResolve(Resolution.Channel, notifier, ResolutionReason.ThalwegChannelCount);
                }
            }

            return;
        }

        foreach (var segment in thalweg.Segments)
        {
            // Invoke rules for individual segments, e.g. no closed loop
            InvokeInternal(segment, notifier);
        }

        var segmentsWithOneTermination = new List<(Tile Tile, int TileCount)>();
        foreach (var segment in thalweg.Segments)
        {
            switch (segment.First, segment.Last)
            {
                case (Tile tile1, Termination _):
                    segmentsWithOneTermination.Add((tile1, segment.TileCount));
                    break;

                case (Termination _, Tile tile2):
                    segmentsWithOneTermination.Add((tile2, segment.TileCount));
                    break;
            }
        }

        foreach (var (tile, tileCount) in segmentsWithOneTermination)
        {
            if (tileCount < thalweg.TileCount)
            {
                // Cannot terminate from this tile as not all channel tiles are accounted for
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
                    // Get common edge to link segments
                    commonEdge.TryResolve(Resolution.Channel, notifier, ResolutionReason.SingleChannel);
                }
                else
                {
                    throw new UnreachableException($"Combined channel segment tile count cannot exceed {thalweg.TileCount}.");
                }
            }
        }
    }

    /*
    private void InvokeInternal(Thalweg thalweg, INotifier notifier)
    {
        var segmentsWithOneTermination = new List<(Tile Tile, int TileCount)>();
        var segments = thalweg.Segments.ToList();

        foreach (var segment in segments)
        {
            // thalweg.Segments can be modified within this loop so we need to be cautious
            if (!thalweg.Segments.Contains(segment))
            {
                continue;
            }

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
                        // Get common edge to link segments
                        commonEdge.TryResolve(Resolution.Channel, notifier, ResolutionReason.SingleChannel);
                    }
                    else
                    {
                        throw new UnreachableException($"Combined channel segment tile count cannot exceed {thalweg.TileCount}.");
                    }
                }
            }
        }
    }
    */

    public override string ToString()
    {
        return nameof(ChannelContinuityRule);
    }
}
