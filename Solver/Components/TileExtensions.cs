using Solver.Framework;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Solver.Components;

public static class TileExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coordinates GetDefaultKey(this Tile tile) => tile.Coordinates;

    public static bool TryGetCommonEdge(this Tile tile, Tile other, out Edge? edge)
    {
        edge = tile.Edges.Intersect(other.Edges).SingleOrDefault();
        return edge is not null;
    }

    public static UnorderedPair<Coordinates> GetEdgeKey(this Tile tile, Axis axis) => axis switch
    {
        Axis.X => new UnorderedPair<Coordinates>(tile.Vertices[1], tile.Vertices[2]),
        Axis.Y => new UnorderedPair<Coordinates>(tile.Vertices[2], tile.Vertices[0]),
        Axis.Z => new UnorderedPair<Coordinates>(tile.Vertices[0], tile.Vertices[1]),
        _ => throw new UnreachableException($"Usupported axis: {axis}")
    };

    public static IEnumerable<Tile> Follow(Tile tile)
    {
        if (tile.Resolution != Resolution.Channel)
        {
            yield break;
        }

        var edges = tile.Edges
            .Where(edge => edge.Resolution == Resolution.Channel)
            .ToArray();

        var edge = (Edge?)null;

        if (edges.Length == 1)
        {
            edge = edges[0];
        }
        else if (edges.Length == 2)
        {
            var stack = new Stack<Tile>();
            edge = edges[0];

            while (!edge.IsBorder)
            {
                var nextTile = edge.Tiles.SingleOrDefault(t => t != tile && t.Resolution == Resolution.Channel);
                if (nextTile is null)
                {
                    break;
                }

                var nextEdge = nextTile.Edges.SingleOrDefault(e => e.NormalAxis != edge.NormalAxis && e.Resolution == Resolution.Channel);
                if (nextEdge is null)
                {
                    break;
                }

                tile = nextTile;
                edge = nextEdge;

                stack.Push(tile);
            }

            while (stack.TryPop(out var channelTile))
            {
                yield return channelTile;
            }

            edge = edges[1];
        }

        yield return tile;

        if (edge is null)
        {
            yield break;
        }

        while (!edge.IsBorder)
        {
            var nextTile = edge.Tiles.SingleOrDefault(t => t != tile && t.Resolution == Resolution.Channel);
            if (nextTile is null)
            {
                break;
            }

            var nextEdge = nextTile.Edges.SingleOrDefault(e => e.NormalAxis != edge.NormalAxis && e.Resolution == Resolution.Channel);
            if (nextEdge is null)
            {
                break;
            }

            tile = nextTile;
            edge = nextEdge;

            yield return tile;
        }
    }

    public static (int ChannelCount, int EmptyCount, int UnknownCount) GetCounts(this Tile tile)
    {
        return tile.Edges.Aggregate(
            (Channel: 0, Empty: 0, Unknown: 0),
            (acc, tile) =>
            {
                switch (tile.Resolution)
                {
                    case Resolution.Unknown:
                        acc.Unknown += 1;
                        break;

                    case Resolution.Channel:
                        acc.Channel += 1;
                        break;

                    case Resolution.Empty:
                        acc.Empty += 1;
                        break;
                }

                return acc;
            });
    }

    public static IEnumerable<Edge> GetEdges(this Tile tile)
    {
        return tile.Edges;
    }

    public static IEnumerable<Edge> GetEdges(this Tile tile, Axis normalAxis)
    {
        return tile.Edges.Where(e => e.NormalAxis != normalAxis);
    }

    public static IEnumerable<Edge> GetPotentiallyLinkableEdges(this Tile tile)
    {
        return tile.Edges.Where(e => e.Resolution != Resolution.Empty);
    }

    public static IEnumerable<Edge> GetPotentiallyLinkableEdges(this Tile tile, Axis normalAxis)
    {
        return tile.Edges.Where(e => e.NormalAxis != normalAxis && e.Resolution != Resolution.Empty);
    }

    public static IEnumerable<Tile> GetAdjacentTiles(this Tile tile)
    {
        return tile.Edges
            .SelectMany(e => e.Tiles.Where(t => t != tile));
    }

    public static IEnumerable<(Edge, Tile)> GetAdjacentComponents(this Tile tile)
    {
        return tile.Edges
            .SelectMany(e => e.Tiles.Where(t => t != tile).Select(t => (e, t)));
    }

    public static IEnumerable<Tile> GetAdjacentTiles(this Tile tile, Axis normalAxis)
    {
        return tile.GetEdges(normalAxis)
            .SelectMany(e => e.Tiles.Where(t => t != tile));
    }

    public static IEnumerable<(Edge, Tile)> GetAdjacentComponents(this Tile tile, Axis normalAxis)
    {
        return tile.GetEdges(normalAxis)
            .SelectMany(e => e.Tiles.Where(t => t != tile).Select(t => (e, t)));
    }

    public static IEnumerable<Tile> GetPotentiallyLinkableTiles(this Tile tile)
    {
        return tile.GetPotentiallyLinkableEdges()
            .SelectMany(e => e.Tiles.Where(t => t != tile));
    }

    public static IEnumerable<(Edge, Tile)> GetPotentiallyLinkableComponents(this Tile tile)
    {
        return tile.GetPotentiallyLinkableEdges()
            .SelectMany(e => e.Tiles.Where(t => t != tile).Select(t => (e, t)));
    }

    public static IEnumerable<Tile> GetPotentiallyLinkableTiles(this Tile tile, Axis normalAxis)
    {
        return tile.GetPotentiallyLinkableEdges(normalAxis)
            .SelectMany(e => e.Tiles.Where(t => t != tile));
    }

    public static IEnumerable<(Edge, Tile)> GetPotentiallyLinkableComponents(this Tile tile, Axis normalAxis)
    {
        return tile.GetPotentiallyLinkableEdges(normalAxis)
            .SelectMany(e => e.Tiles.Where(t => t != tile).Select(t => (e, t)));
    }
}
