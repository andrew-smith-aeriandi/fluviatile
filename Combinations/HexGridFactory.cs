using System;
using System.Collections.Generic;
using System.Linq;

namespace Combinations
{
    public class HexGridFactory
    {
        public Tableau<Coordinate> Create(int n)
        {
            var m = 3 * n;
            var xrange = Enumerable.Range(-m, 2 * m + 1);
            var yrange = Enumerable.Range(-m, 2 * m + 1);

            bool IsVertex(Coordinate vertex)
            {
                return (vertex.X + vertex.Y) % 3 == 0 &&
                       vertex.X % 3 != 0 &&
                       vertex.Y > vertex.X - m &&
                       vertex.Y < vertex.X + m;
            }

            var tiles = xrange
                .SelectMany(x => yrange, (x, y) => new Coordinate(x, y))
                .Where(IsVertex)
                .ToDictionary(c => c, c => (Node)new Tile(c));

            IEnumerable<Coordinate> Links(Coordinate vertex)
            {
                switch ((vertex.X + m) % 3)
                {
                    case 1:
                        yield return new Coordinate(vertex.X + 1, vertex.Y - 1);
                        yield return new Coordinate(vertex.X + 1, vertex.Y + 2);
                        yield return new Coordinate(vertex.X - 2, vertex.Y - 1);
                        break;

                    case 2:
                        yield return new Coordinate(vertex.X - 1, vertex.Y + 1);
                        yield return new Coordinate(vertex.X - 1, vertex.Y - 2);
                        yield return new Coordinate(vertex.X + 2, vertex.Y + 1);
                        break;
                }
            }

            var endpoints = new Dictionary<Coordinate, Node>();

            foreach (var tile in tiles.Values)
            {
                tile.AddCountIndex(
                    tile.Value.X >= 0
                        ? tile.Value.X / 3 + 5 * n
                        : -tile.Value.X / 3 + 2 * n);

                tile.AddCountIndex(
                    tile.Value.Y >= 0
                        ? tile.Value.Y / 3
                        : -tile.Value.Y / 3 + 3 * n);

                tile.AddCountIndex(
                    tile.Value.Y >= tile.Value.X
                        ? (tile.Value.Y - tile.Value.X) / 3 + n
                        : (tile.Value.X - tile.Value.Y) / 3 + 4 * n);

                foreach (var link in Links(tile.Value))
                {
                    if (!tiles.TryGetValue(link, out var linkedNode))
                    {
                        if (!endpoints.TryGetValue(link, out linkedNode))
                        {
                            linkedNode = new TerminalNode(link);
                            endpoints.Add(link, linkedNode);
                        }
                    }

                    tile.AddLink(linkedNode);
                    linkedNode.AddLink(tile);
                }
            }

            return new Tableau<Coordinate>(tiles.Values, endpoints.Values);
        }

    }
}
