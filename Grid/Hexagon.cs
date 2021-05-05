using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluviatile.Grid
{
    public class Hexagon : Shape
    {
        private readonly int[] _nodeCountMap;

        public static readonly Torsion Left = new Torsion(-1);
        public static readonly Torsion Right = new Torsion(1);
        public static readonly Torsion UTurn = new Torsion(3);

        public static Direction Azimuth240 = new Direction(0);
        public static Direction Azimuth300 = new Direction(1);
        public static Direction Azimuth000 = new Direction(2);
        public static Direction Azimuth060 = new Direction(3);
        public static Direction Azimuth120 = new Direction(4);
        public static Direction Azimuth180 = new Direction(5);

        private static readonly Dictionary<Direction, Coordinates> Vectors = new Dictionary<Direction, Coordinates>
        {
            [Azimuth240] = new Coordinates(-1, 1),
            [Azimuth300] = new Coordinates(-2, -1),
            [Azimuth000] = new Coordinates(-1, -2),
            [Azimuth060] = new Coordinates(1, -1),
            [Azimuth120] = new Coordinates(2, 1),
            [Azimuth180] = new Coordinates(1, 2)
        };

        private static readonly Dictionary<int, Direction[]> Directions = new Dictionary<int, Direction[]>
        {
            [1] = new[] { Azimuth300, Azimuth060, Azimuth180 },
            [2] = new[] { Azimuth240, Azimuth000, Azimuth120 }
        };

        private static int[] BuildNodeCountMap(int size, int scale)
        {
            var m = 2 * size;
            var n = scale * m;
            var map = new int[n];

            for (var i = 0; i < m; i++)
            {
                for (var j = 0; j < scale; j++)
                {
                    map[scale * i + j] = i;
                }
            }

            return map;
        }

        public Hexagon(int size) : base(
            name: typeof(Hexagon).Name,
            edges: 6,
            size: size,
            scale: 3)
        {
            _nodeCountMap = BuildNodeCountMap(Size, Scale);
        }

        public override Coordinates Centre => new Coordinates(Size * Scale, Size * Scale);

        public override IEnumerable<Func<Coordinates, Coordinates>> SymmetryTransformations()
        {
            yield return (Coordinates p) => new Coordinates(p.X, p.Y);
            yield return (Coordinates p) => new Coordinates(p.X - p.Y, p.X);
            yield return (Coordinates p) => new Coordinates(-p.Y, p.X - p.Y);
            yield return (Coordinates p) => new Coordinates(-p.X, -p.Y);
            yield return (Coordinates p) => new Coordinates(-p.X + p.Y, -p.X);
            yield return (Coordinates p) => new Coordinates(p.Y, -p.X + p.Y);
            yield return (Coordinates p) => new Coordinates(p.X - p.Y, -p.Y);
            yield return (Coordinates p) => new Coordinates(p.X, p.X - p.Y);
            yield return (Coordinates p) => new Coordinates(p.Y, p.X);
            yield return (Coordinates p) => new Coordinates(-p.X + p.Y, p.Y);
            yield return (Coordinates p) => new Coordinates(-p.X, -p.X + p.Y);
            yield return (Coordinates p) => new Coordinates(-p.Y, -p.X);
        }

        public override Tableau CreateTableau()
        {
            var nodes = new Dictionary<Coordinates, Node>();
            var terminalNodes = new Dictionary<Coordinates, TerminalNode>();

            var offset = new Coordinates(1, -1);
            var nodeCount = 0;
            var terminalNodeCount = 0;

            for (var shell = 0; shell < Size; shell++)
            {
                var position = new Coordinates((Size + shell) * Scale, Size * Scale) + offset;
                var direction = Azimuth180;

                for (var sector = 0; sector < Edges; sector++)
                {
                    var n = 2 * shell + 1;
                    for (var i = 0; i < n; i++)
                    {
                        position += Vectors[direction];
                        nodes.Add(position, new Node(nodeCount, position));
                        nodeCount += 1;

                        direction += (i & 1) == 0 ? Right : Left;
                    }
                }
            }

            foreach (var node in nodes.Values)
            {
                var links = Directions[(node.Coordinates.X + Scale) % Scale]
                    .Select(direction =>
                    {
                        var coordinates = node.Coordinates + Vectors[direction];
                        if (nodes.TryGetValue(coordinates, out var linkNode))
                        {
                            return (direction, linkNode);
                        }

                        var terminalNode = new TerminalNode(
                            terminalNodeCount,
                            coordinates,
                            new[] { (direction.Turn(UTurn), node) });

                        terminalNodes.Add(coordinates, terminalNode);
                        terminalNodeCount += 1;

                        return (direction, terminalNode);
                    });

                node.AddLinks(links);
            }

            return new Tableau(
                this,
                nodes.Values,
                terminalNodes.Values);
        }

        public override bool IsFullTurn(Step step1, Step step2)
        {
            var delta = step1.Torsion - step2.Torsion;
            return Math.Abs(delta.Value) >= Edges;
        }

        public override int[] CountNodes(Step path)
        {
            var counts = new int[Size * Edges];
            var del = Scale * Size;
            var m = 2 * Size;

            var yOffset = m;
            var xyOffset = 2 * m;

            var step = path.Previous;
            while (step is Step)
            {
                var x = step.Node.Coordinates.X;
                var y = step.Node.Coordinates.Y;

                counts[_nodeCountMap[x]]++;
                counts[_nodeCountMap[y] + yOffset]++;
                counts[_nodeCountMap[y - x + del] + xyOffset]++;

                step = step.Previous;
            }

            return counts;
        }
    }
}
