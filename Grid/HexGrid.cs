using Fluviatile.Grid.Random;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Fluviatile.Grid
{
    public class HexGrid : IGrid
    {
        private readonly IRandom _random;
        private readonly int _minSteps;

        private const float Delta = 0.1f;

        private List<(int x, int y)> _sequence;
        private List<int> _nodeCounts;

        public HexGrid(int size, double minStepsPercentage, IRandom random)
        {
            Size = size;
            _minSteps = (int)(minStepsPercentage * 6 * size * size);
            _random = random;
        }

        public int Size { get; }

        public void SetSequence(IEnumerable<(int x, int y)> sequence)
        {
            Interlocked.Exchange(ref _sequence, sequence.ToList());
        }

        public void SetNodeCounts(IEnumerable<int> nodeCounts)
        {
            Interlocked.Exchange(ref _nodeCounts, nodeCounts.ToList());
        }

        public void CreateSequence()
        {
            const int MaxAttempts = 1000;

            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                if (TryCreateSequence(_minSteps, out var sequence))
                {
                    Interlocked.Exchange(ref _sequence, sequence);
                    return;
                }
            }

            throw new Exception("Failed to create sequence");
        }

        public IEnumerable<(int x, int y)> Sequence()
        {
            return _sequence;
        }

        public IEnumerable<IEnumerable<(float x, float y)>> GetMargins()
        {
            var n1 = (float)Size;
            var n2 = (float)(Size * 2);

            // x + y direction
            yield return new List<(float, float)>
            {
                (n2 + Delta, n1 + Delta),
                (n2 + Delta, n2 + Delta),
                (n1 + Delta, n2 + Delta),
                (n1 + 1f + Delta, n2 + 1f + Delta),
                (n2 + 1f + Delta, n2 + 1f + Delta),
                (n2 + 1f + Delta, n1 + 1f + Delta)
            };

            // -x direction
            yield return new List<(float, float)>
            {
                (n1 - Delta, n2),
                (0f - Delta, n1),
                (0f - Delta, 0f),
                (0f - 1f - Delta, 0f),
                (0f - 1f - Delta, n1),
                (n1 - 1f - Delta, n2)
            };

            // -y direction
            yield return new List<(float, float)>
            {
                (0f, 0f - Delta),
                (n1, 0f - Delta),
                (n2, n1 - Delta),
                (n2, n1 - 1f - Delta),
                (n1, 0f - 1f - Delta),
                (0f, 0f - 1f - Delta)
            };
        }

        public IEnumerable<((float x, float y) from, (float x, float y) to)> MarginLines()
        {
            var n1 = (float)Size;
            var n2 = (float)(Size * 2);

            for (var x = 0; x <= n2; x += 1)
            {
                var y = Math.Max(0f, x - n1) - Delta;
                yield return (
                    (x, y),
                    (x, y - 1f));
            }

            for (var y = 0; y <= n2; y += 1)
            {
                var x = Math.Max(0f, y - n1) - Delta;
                yield return (
                    (x, y),
                    (x - 1f, y));
            }

            for (var z = 0; z <= n2; z += 1)
            {
                var x = Math.Min(n2, n2 + n1 - z) + Delta;
                var y = Math.Min(n2, n1 + z) + Delta;
                yield return (
                    (x, y),
                    (x + 1f, y + 1f));
            }
        }

        public IEnumerable<((int x, int y) position, IEnumerable<(float x, float y)> polygon)> GridCells()
        {
            var size3 = Size * 3;
            var size6 = Size * 6;
            var xrange = Enumerable.Range(1, size6);
            var yrange = Enumerable.Range(1, size6);

            static bool IsVertex((int x, int y) vertex)
            {
                return (vertex.x + vertex.y) % 3 == 0 &&
                    vertex.x % 3 != 0;
            }

            var vertices = xrange
                .SelectMany(x => yrange, (x, y) => (x, y))
                    .Where(vertex => Math.Abs(vertex.y - vertex.x) <= size3)
                    .Where(IsVertex)
                    .ToList();

            foreach (var (x, y) in vertices)
            {
                if (x % 3 == 1)
                {
                    // Triangle with vertex pointing upwards
                    yield return (
                        position: (x, y),
                        polygon: new List<(float x, float y)>
                        {
                            ((x - 1) / 3f, (y - 2) / 3f),
                            ((x + 2) / 3f, (y + 1) / 3f),
                            ((x - 1) / 3f, (y + 1) / 3f)
                        });
                }
                else if (x % 3 == 2)
                {
                    // Triangle with vertex pointing downwards
                    yield return (
                        position: (x, y),
                        polygon: new List<(float x, float y)>
                        {
                            ((x + 1) / 3f, (y + 2) / 3f),
                            ((x - 2) / 3f, (y - 1) / 3f),
                            ((x + 1) / 3f, (y - 1) / 3f)
                        });
                }
            }
        }

        public IEnumerable<((float x, float y) from, (float x, float y) to)> GridLines()
        {
            var n1 = (float)Size;
            var n2 = (float)(Size * 2);

            for (var x = 0; x <= 2 * Size; x += 1)
            {
                yield return (
                    (x, Math.Max(0f, x - n1)),
                    (x, Math.Min(n2 , x + n1)));
            }

            for (var y = 0; y <= 2 * Size; y += 1)
            {
                yield return (
                    (Math.Max(0f, y - n1), y),
                    (Math.Min(y + n1, n2), y));
            }

            for (var z = 0; z <= 2 * Size; z += 1)
            {
                yield return (
                    (Math.Min(n2, n2 + n1 - z), Math.Min(n2, n1 + z)),
                    (Math.Max(0f, n1 - z), Math.Max(0f, z - n1)));
            }
        }

        public IEnumerable<(string group, int index, float x, float y, int count, int max)> NodeCounts()
        {
            var n1 = (float)Size;
            var n2 = (float)(Size * 2);
            const float half = 0.5F;

            var hasNodeCounts = _nodeCounts?.Count == Size * 6;
            var nodeCountIndex = 0;

            for (var z = 0; z < 2 * Size; z += 1)
            {
                var maxNodes = z < Size
                    ? 2 * (z + Size) + 1
                    : 2 * (3 * Size - z) - 1;

                var nodeCount = hasNodeCounts
                    ? _nodeCounts[nodeCountIndex++]
                    : _sequence.Count(vertex => z == (Size * 3 - vertex.x + vertex.y) / 3);

                yield return (
                    group: "z",
                    index: z,
                    x: Math.Min(n2 + half, n2 + n1 - z) + Delta,
                    y: Math.Min(n1 + 1 + z, n2 + half) + Delta,
                    count: nodeCount,
                    max: maxNodes);
            }

            for (var y = 2 * Size - 1; y >= 0; y--)
            {
                var maxNodes = y < Size
                    ? 2 * (y + Size) + 1
                    : 2 * (3 * Size - y) - 1;

                var nodeCount = hasNodeCounts
                    ? _nodeCounts[nodeCountIndex++]
                    : _sequence.Count(vertex => y == vertex.y / 3);

                yield return (
                    group: "y",
                    index: y,
                    x: Math.Max(-half, y - n1) - Delta,
                    y: y + half,
                    count: nodeCount,
                    max: maxNodes);
            }

            for (var x = 0; x < 2 * Size; x++)
            {
                var maxNodes = x < Size
                    ? 2 * (x + Size) + 1
                    : 2 * (3 * Size - x) - 1;

                var nodeCount = hasNodeCounts
                    ? _nodeCounts[nodeCountIndex++]
                    : _sequence.Count(vertex => x == vertex.x / 3);

                yield return (
                    group: "x",
                    index: x,
                    x: x + half,
                    y: Math.Max(-half, x - n1) - Delta,
                    count: nodeCount,
                    max: maxNodes);
            }
        }

        private bool TryCreateSequence(int minSteps, out List<(int x, int y)> sequence)
        {
            sequence = new List<(int x, int y)>();

            var size3 = Size * 3;
            var size6 = Size * 6;
            var xrange = Enumerable.Range(1, size6);
            var yrange = Enumerable.Range(1, size6);

            var vertexes = new HashSet<(int x, int y)>(
                xrange.SelectMany(x => yrange, (x, y) => (x, y))
                    .Where(vertex => Math.Abs(vertex.y - vertex.x) <= size3)
                    .Where(IsVertex));

            bool IsVertex((int x, int y) vertex)
            {
                return (vertex.x + vertex.y) % 3 == 0 &&
                    vertex.x % 3 != 0;
            }

            bool IsOnEdge((int x, int y) vertex)
            {
                return vertex.x == 1 ||
                    vertex.x == size6 - 1 ||
                    vertex.y == 1 ||
                    vertex.y == size6 - 1 ||
                    Math.Abs(vertex.y - vertex.x) == size3 - 1;
            }

            IEnumerable<(int x, int y)> AdjacentVertices((int x, int y) vertex)
            {
                if (vertex.x % 3 == 1)
                {
                    yield return (vertex.x + 1, vertex.y - 1);

                    if (vertex.x > 3)
                    {
                        yield return (vertex.x - 2, vertex.y - 1);
                    }

                    if (vertex.y < size6 - 3)
                    {
                        yield return (vertex.x + 1, vertex.y + 2);
                    }
                }
                else if (vertex.x % 3 == 2)
                {
                    yield return (vertex.x - 1, vertex.y + 1);

                    if (vertex.y > 3)
                    {
                        yield return (vertex.x - 1, vertex.y - 2);
                    }

                    if (vertex.x < size6 - 3)
                    {
                        yield return (vertex.x + 2, vertex.y + 1);
                    }
                }
            }

            var edges = vertexes.Where(IsOnEdge).ToList();
            var position = edges[_random.Choose(edges.Count)];

            sequence.Add(position);
            vertexes.Remove(position);

            while (true)
            {
                var possibilities = AdjacentVertices(position).Where(vertex => vertexes.Contains(vertex)).ToList();
                if (possibilities.Count == 0)
                {
                    return IsOnEdge(position) && sequence.Count >= minSteps;
                }

                position = possibilities[_random.Choose(possibilities.Count)];
                sequence.Add(position);
                vertexes.Remove(position);

                if (IsOnEdge(position) && _random.Try(0.25))
                {
                    return sequence.Count >= minSteps;
                }
            }
        }

        public string DisplayText()
        {
            return _random.Seed.ToString();
        }

        public void Dump()
        {
            foreach (var (x, y) in _sequence)
            {
                Debug.WriteLine($"({x}, {y})");
            }

            Debug.WriteLine($"Count: {_sequence.Count}");
        }
   }
}
