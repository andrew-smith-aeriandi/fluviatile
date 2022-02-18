using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluviatile.Grid
{
    public class Hexagon : Shape
    {
        private static readonly IComparer<byte[]> CounterComparer = new ByteArrayComparer();

        public static readonly Torsion Left = new Torsion(-1);
        public static readonly Torsion Right = new Torsion(1);
        public static readonly Torsion UTurn = new Torsion(3);

        public static Direction Azimuth000 = new Direction(0);
        public static Direction Azimuth060 = new Direction(1);
        public static Direction Azimuth120 = new Direction(2);
        public static Direction Azimuth180 = new Direction(3);
        public static Direction Azimuth240 = new Direction(4);
        public static Direction Azimuth300 = new Direction(5);

        private static readonly Dictionary<Direction, Coordinates> Vectors = new()
        {
            [Azimuth000] = new Coordinates(-1, -2),
            [Azimuth060] = new Coordinates(1, -1),
            [Azimuth120] = new Coordinates(2, 1),
            [Azimuth180] = new Coordinates(1, 2),
            [Azimuth240] = new Coordinates(-1, 1),
            [Azimuth300] = new Coordinates(-2, -1)
        };

        private static readonly Dictionary<int, Direction[]> Directions = new()
        {
            [1] = new[] { Azimuth060, Azimuth180, Azimuth300 },
            [2] = new[] { Azimuth000, Azimuth120, Azimuth240 }
        };

        private Func<int, int> ConvertToIndex { get; }

        public Hexagon(int size) : base(
            name: typeof(Hexagon).Name,
            edges: 6,
            size: size,
            scale: 3)
        {
            ConvertToIndex = (coordinate) => (coordinate >= 0 ? coordinate : coordinate - Scale + 1) / Scale;
            Centre = new Coordinates(Size * Scale, Size * Scale);
            Tiles = 6 * size * size;
            SymmetryTransformations = GetSymmetryTransforms(Centre).ToList();
        }

        /// <summary>
        /// Coordinates of the centre of the shape
        /// </summary>
        public override Coordinates Centre { get; }

        public override int[][] NodeCountPermutations { get; set; }

        /// <summary>
        /// Total number of tiles in hexagonal grid
        /// </summary>
        public override int Tiles { get; }

        public override IEnumerable<ISymmetryTransform> SymmetryTransformations { get; }

        public override Tableau CreateTableau()
        {
            var gridNodes = new Dictionary<Coordinates, GridNode>();
            var terminalNodes = new Dictionary<Coordinates, TerminalNode>();
            var terminalNodeLookupByAisle = new Dictionary<Aisle, TerminalNode>();

            var nodeCount = 0;
            var terminalNodeCount = 0;

            for (var shell = 0; shell < Size; shell++)
            {
                var position = Centre - new Coordinates(shell * Scale + 2, shell * Scale + 1);
                var direction = Azimuth060;

                for ( var sector = 0; sector < Edges; sector++)
                {
                    var n = 2 * shell + 1;
                    for (var i = 0; i < n; i++)
                    {
                        position += Vectors[direction];
                        gridNodes.Add(position, new GridNode(nodeCount, position));
                        nodeCount += 1;

                        direction += (i & 1) == 0 ? Right : Left;
                    }
                }
            }

            foreach (var gridNode in gridNodes.Values)
            {
                var links = Directions[(gridNode.Coordinates.X + Scale) % Scale]
                    .Select(direction =>
                    {
                        var coordinates = gridNode.Coordinates + Vectors[direction];
                        if (gridNodes.TryGetValue(coordinates, out var linkNode))
                        {
                            return (direction, linkNode);
                        }

                        var candidateAisles = GetAisles(coordinates).ToList();
                        var outOfRangeIndex = candidateAisles.FindIndex(a => !(a.Index >= 0 && a.Index < 2 * Size));
                        var aisle = candidateAisles[(outOfRangeIndex + 2) % 3];

                        var terminalNode = new TerminalNode(
                            terminalNodeCount,
                            coordinates,
                            new[] { (direction.Turn(UTurn), (Node)gridNode) },
                            aisle);

                        terminalNodes.Add(coordinates, terminalNode);
                        terminalNodeLookupByAisle.Add(aisle, terminalNode);
                        terminalNodeCount += 1;

                        return (direction, (Node)terminalNode);
                    });

                gridNode.AddLinks(links);
            }

            foreach (var gridNode in gridNodes.Values)
            {
                gridNode.AddCounterIndexes(
                    GetAisles(gridNode.Coordinates)
                        .Select(aisle => terminalNodeLookupByAisle[aisle])
                        .Select(node => node.Index));
            }

            /*
            var perms = SymmetryTransformations
                .Select(transformation => terminalNodes.Values
                    .Select(node => terminalNodes[transformation.Transform(node.Coordinates)].Index)
                    .ToArray())
                .ToArray();
            */

            // TODO: write algorithm to calculate this for any sixe
            NodeCountPermutations = Size switch
            {
                1 => new int[][]
                {
                    new int[] { 0, 1, 2, 3, 4, 5 },
                    new int[] { 5, 0, 1, 2, 3, 4 },
                    new int[] { 4, 5, 0, 1, 2, 3 },
                    new int[] { 3, 4, 5, 0, 1, 2 },
                    new int[] { 2, 3, 4, 5, 0, 1 },
                    new int[] { 1, 2, 3, 4, 5, 0 },
                    new int[] { 4, 3, 2, 1, 0, 5 },
                    new int[] { 0, 5, 4, 3, 2, 1 },
                    new int[] { 2, 1, 0, 5, 4, 3 },
                    new int[] { 1, 0, 5, 4, 3, 2 },
                    new int[] { 5, 4, 3, 2, 1, 0 },
                    new int[] { 3, 2, 1, 0, 5, 4 }
                },
                2 => new int[][]
                {
                    new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                    new int[] { 10, 11, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    new int[] { 8, 9, 10, 11, 0, 1, 2, 3, 4, 5, 6, 7 },
                    new int[] { 6, 7, 8, 9, 10, 11, 0, 1, 2, 3, 4, 5 },
                    new int[] { 4, 5, 6, 7, 8, 9, 10, 11, 0, 1, 2, 3 },
                    new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 0, 1 },
                    new int[] { 8, 9, 6, 7, 4, 5, 2, 3, 0, 1, 10, 11 },
                    new int[] { 0, 1, 10, 11, 8, 9, 6, 7, 4, 5, 2, 3 },
                    new int[] { 4, 5, 2, 3, 0, 1, 10, 11, 8, 9, 6, 7 },
                    new int[] { 2, 3, 0, 1, 10, 11, 8, 9, 6, 7, 4, 5 },
                    new int[] { 10, 11, 8, 9, 6, 7, 4, 5, 2, 3, 0, 1 },
                    new int[] { 6, 7, 4, 5, 2, 3, 0, 1, 10, 11, 8, 9 }
                },
                3 => new int[][]
                {
                    new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 },
                    new int[] { 15, 16, 17, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 },
                    new int[] { 12, 13, 14, 15, 16, 17, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                    new int[] { 9, 10, 11, 12, 13, 14, 15, 16, 17, 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                    new int[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 0, 1, 2, 3, 4, 5 },
                    new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 0, 1, 2 },
                    new int[] { 12, 13, 14, 9, 10, 11, 6, 7, 8, 3, 4, 5, 0, 1, 2, 15, 16, 17 },
                    new int[] { 0, 1, 2, 15, 16, 17, 12, 13, 14, 9, 10, 11, 6, 7, 8, 3, 4, 5 },
                    new int[] { 6, 7, 8, 3, 4, 5, 0, 1, 2, 15, 16, 17, 12, 13, 14, 9, 10, 11 },
                    new int[] { 3, 4, 5, 0, 1, 2, 15, 16, 17, 12, 13, 14, 9, 10, 11, 6, 7, 8 },
                    new int[] { 15, 16, 17, 12, 13, 14, 9, 10, 11, 6, 7, 8, 3, 4, 5, 0, 1, 2 },
                    new int[] { 9, 10, 11, 6, 7, 8, 3, 4, 5, 0, 1, 2, 15, 16, 17, 12, 13, 14 }
                },
                _ => throw new InvalidOperationException($"Size {Size} not supported")
            };

            return new Tableau(
                this,
                gridNodes.Values,
                terminalNodes.Values);
        }

        public override bool IsFullTurn(Step step1, Step step2)
        {
            var delta = step1.Torsion - step2.Torsion;
            return Math.Abs(delta.Value) >= Edges;
        }

        public IEnumerable<Aisle> GetAisles(Coordinates position)
        {
            yield return new Aisle(0, ConvertToIndex(position.X));
            yield return new Aisle(1, ConvertToIndex(position.Y));
            yield return new Aisle(2, ConvertToIndex(position.Y - position.X + Size * Scale));
        }

        public override byte[] CountNodesAndNormalise(Step path, int[][] permutations)
        {
            var nodeCounts = CountNodes(path);

            return permutations
                .Select(map =>
                {
                    var permutation = new byte[nodeCounts.Length];
                    for (var i = 0; i < permutation.Length; i++)
                    {
                        permutation[i] = nodeCounts[map[i]];
                    }

                    return permutation;
                })
                .OrderBy(arr => arr, CounterComparer)
                .First();
        }

        public override byte[] CountNodes(Step path)
        {
            var counts = new byte[6 * Size];
            var step = path;

            while (step is not null)
            {
                if (step.Node is GridNode gridNode)
                {
                    foreach (var index in gridNode.CounterIndexes)
                    {
                        counts[index]++;
                    }
                }

                step = step.Previous;
            }

            return counts;
        }

        private static IEnumerable<ISymmetryTransform> GetSymmetryTransforms(Coordinates centre)
        {
            yield return new SymmetryTransform(
                "Identity",
                SymmetryType.Identity,
                p => new Coordinates(p.X, p.Y));

            yield return new SymmetryTransform(
                "Rotate60",
                SymmetryType.Rotation,
                p => new Coordinates(p.X - p.Y + centre.Y, p.X - centre.X + centre.Y));

            yield return new SymmetryTransform(
                "Rotate120",
                SymmetryType.Rotation,
                p => new Coordinates(-p.Y + centre.X + centre.Y, p.X - p.Y - centre.X + 2 * centre.Y));

            yield return new SymmetryTransform(
                "Rotate180",
                SymmetryType.Rotation,
                p => new Coordinates(-p.X + 2 * centre.X, -p.Y + 2 * centre.Y));

            yield return new SymmetryTransform(
                "Rotate240",
                SymmetryType.Rotation,
                p => new Coordinates(-p.X + p.Y + 2 * centre.X - centre.Y, -p.X + centre.X + centre.Y));

            yield return new SymmetryTransform(
                "Rotate300",
                SymmetryType.Rotation,
                p => new Coordinates(p.Y + centre.X - centre.Y, -p.X + p.Y + centre.X));

            yield return new SymmetryTransform(
                "MirrorX",
                SymmetryType.Mirror,
                p => new Coordinates(p.X - p.Y + centre.Y, -p.Y + 2 * centre.Y));

            yield return new SymmetryTransform(
                "MirrorY",
                SymmetryType.Mirror,
                p => new Coordinates(-p.X + 2 * centre.X, -p.X + p.Y + centre.X));

            yield return new SymmetryTransform(
                "MirrorZ",
                SymmetryType.Mirror,
                p => new Coordinates(p.Y + centre.X - centre.Y, p.X - centre.X + centre.Y));

            yield return new SymmetryTransform(
                "Mirror30",
                SymmetryType.Mirror,
                p => new Coordinates(p.X, p.X - p.Y - centre.X + 2 * centre.Y));

            yield return new SymmetryTransform(
                "Mirror90",
                SymmetryType.Mirror,
                p => new Coordinates(-p.X + p.Y + 2 * centre.X - centre.Y, p.Y));

            yield return new SymmetryTransform(
                "Mirror150",
                SymmetryType.Mirror,
                p => new Coordinates(-p.Y + centre.X + centre.Y, -p.X + centre.X + centre.Y));
        }

        public override Func<Step, IEnumerable<byte[]>> GetEquivalentPathsDelegate(NodePair nodePair)
        {
            var symmetryTypes = GetSymmetryTransforms(Centre)
                .Where(symmetry =>
                    nodePair.Node1.Coordinates == symmetry.Transform(nodePair.Node2.Coordinates) &&
                    nodePair.Node2.Coordinates == symmetry.Transform(nodePair.Node1.Coordinates))
                .Aggregate(SymmetryType.Identity, (result, symmetry) => result | symmetry.SymmetryType);

            return symmetryTypes switch
            {
                SymmetryType.Identity => StepExtensions.GetEquivalentPathsWithIdentitySymmetry,
                SymmetryType.Mirror => StepExtensions.GetEquivalentPathsWithMirrorSymmetry,
                SymmetryType.Rotation => StepExtensions.GetEquivalentPathsWithRotationalSymmetry,
                SymmetryType.Mirror | SymmetryType.Rotation => StepExtensions.GetEquivalentPathsWithMirrorAndRotationalSymmetry,
                _ => throw new NotImplementedException($"{nameof(GetEquivalentPathsDelegate)} does not support node pairs with {symmetryTypes} symmetrie(s).")
            };
        }
    }
}
