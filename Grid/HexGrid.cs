using Fluviatile.Grid.Random;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Fluviatile.Grid
{
    public class HexGrid : IGrid
    {
        private readonly IRandom _random;
        private readonly int _minSteps;

        private const float Zero = 0f;
        private const float One = 1f;
        private const float Third = One / 3f;
        private const float Delta = 0.1f;

        private const float DefaultMinStepsPercentage = 0.3f;

        private List<(int x, int y)> _sequence;
        private List<int> _nodeCounts;
        private List<NodeState> _initialState;

        public HexGrid(int size)
        {
            _minSteps = (int)(DefaultMinStepsPercentage * 6 * size * size);
            _random = null;

            Size = size;
            DisplayText = $"Hexagon-{size}";
        }

        public HexGrid(int size, double minStepsPercentage, IRandom random)
        {
            _minSteps = (int)(minStepsPercentage * 6 * size * size);
            _random = random;

            Size = size;
            DisplayText = $"Hexagon-{size}; Seed: {_random.Seed}";
        }

        public int Size { get; }

        public string DisplayText { get; private set; }

        public void Dump()
        {
            foreach (var (x, y) in _sequence)
            {
                Debug.WriteLine($"({x}, {y})");
            }

            Debug.WriteLine($"Count: {_sequence.Count}");
        }

        public void SetSequence(IEnumerable<(int x, int y)> sequence)
        {
            Interlocked.Exchange(ref _sequence, [.. sequence]);
            DisplayText = $"Hexagon-{Size}";
        }

        public void SetNodeCounts(IEnumerable<int> nodeCounts)
        {
            Interlocked.Exchange(ref _nodeCounts, [.. nodeCounts]);
            DisplayText = $"[{string.Join(", ", _nodeCounts)}]";
        }

        public void SetInitialState(IEnumerable<NodeState> state)
        {
            _initialState = [.. state];
        }

        public IEnumerable<NodeState> GetInitialState()
        {
            return _initialState;
        }

        public void CreateSequence()
        {
            const int MaxAttempts = 1000;

            var random = _random ?? new Pseudorandom(Environment.TickCount);

            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                if (TryCreateSequence(Size, _minSteps, random, out var sequence))
                {
                    Interlocked.Exchange(ref _sequence, sequence);
                    DisplayText = $"Hexagon-{Size}; Seed: {random.Seed}";
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
            var size = (float)Size;

            // x direction
            yield return new List<(float, float)>
            {
                (-size, -Delta),
                (Zero, -size - Delta),
                (size, -size - Delta),
                (size, -size - One - Delta),
                (Zero, -size - One - Delta),
                (-size, -One - Delta)
            };

            // y direction
            yield return new List<(float, float)>
            {
                (size + Delta, -size),
                (size + Delta, Zero),
                (Delta, size),
                (One + Delta, size),
                (size + One + Delta, Zero),
                (size + One + Delta, -size)
            };

            // z direction
            yield return new List<(float, float)>
            {
                (-Delta, size + Delta),
                (-size - Delta, size + Delta),
                (-size - Delta, Delta),
                (-size - One - Delta, One + Delta),
                (-size - One - Delta, size + One + Delta),
                (-One - Delta, size + One + Delta)
            };
        }

        public IEnumerable<((float x, float y) from, (float x, float y) to)> MarginLines()
        {
            var size = (float)Size;

            // x direction
            for (var x = -size; x <= size; x += 1)
            {
                var y = Math.Max(-size, -size - x) - Delta;
                yield return (
                    (x, y),
                    (x, y - One));
            }

            // y direction
            for (var y = -size; y <= size; y += 1)
            {
                var x = Math.Min(size, size - y) + Delta;
                yield return (
                    (x, y),
                    (x + One, y));
            }

            // z direction
            for (var z = -size; z <= size; z += 1)
            {
                var x = Math.Max(-size, -size - z) - Delta;
                var y = Math.Min(size, size - z) + Delta;
                yield return (
                    (x, y),
                    (x - One, y + One));
            }
        }

        public IEnumerable<((int x, int y) position, IEnumerable<(float x, float y)> polygon)> GridCells()
        {
            var size3 = Size * 3;
            var size6 = Size * 6;
            var xrange = Enumerable.Range(1 - size3, size6);
            var yrange = Enumerable.Range(1 - size3, size6);

            static bool IsVertex((int x, int y) vertex)
            {
                return (vertex.x - vertex.y) % 3 == 0 && vertex.x % 3 != 0;
            }

            var vertices = xrange
                .SelectMany(x => yrange, (x, y) => (x, y))
                    .Where(vertex => Math.Abs(vertex.x + vertex.y) <= size3)
                    .Where(IsVertex)
                    .ToList();

            foreach (var (x, y) in vertices)
            {
                if ((x + size3) % 3 == 1)
                {
                    // Triangle with vertex pointing downwards
                    yield return (
                        position: (x, y),
                        polygon: new List<(float x, float y)>
                        {
                            ((x - 1) * Third, (y - 1) * Third),
                            ((x + 2) * Third, (y - 1) * Third),
                            ((x - 1) * Third, (y + 2) * Third)
                        });
                }
                else if ((x + size3) % 3 == 2)
                {
                    // Triangle with vertex pointing upwards
                    yield return (
                        position: (x, y),
                        polygon: new List<(float x, float y)>
                        {
                            ((x + 1) * Third, (y + 1) * Third),
                            ((x - 2) * Third, (y + 1) * Third),
                            ((x + 1) * Third, (y - 2) * Third)
                        });
                }
            }
        }

        public IEnumerable<((float x, float y) from, (float x, float y) to)> GridLines()
        {
            var size = (float)Size;

            // x direction
            for (var x = -size; x <= size; x++)
            {
                yield return (
                    (x, Math.Max(-size, -size - x)),
                    (x, Math.Min(size, size - x)));
            }

            // y direction
            for (var y = -size; y <= size; y++)
            {
                yield return (
                    (Math.Min(size, size - y), y),
                    (Math.Max(-size, -size - y), y));
            }

            // z direction
            for (var z = -size; z <= size; z++)
            {
                yield return (
                    (Math.Max(-size, -size - z), Math.Min(size, size - z)),
                    (Math.Min(size, size - z), Math.Max(-size, -size - z)));
            }
        }

        public IEnumerable<(string group, int index, float x, float y, int count, int max)> NodeCounts()
        {
            const float half = 0.5f;

            var n1 = (float)Size;
            var n2 = (float)(Size * 2);

            var hasNodeCounts = _nodeCounts?.Count == Size * 6;
            var nodeCountIndex = 0;

            // x direction
            for (var index = 0; index < 2 * Size; index++)
            {
                var nodeCount = hasNodeCounts
                    ? _nodeCounts[nodeCountIndex++]
                    : _sequence.Count(vertex => index == (vertex.x + Size) / 3);

                yield return (
                    group: "x",
                    index: index,
                    x: index - n1 + half,
                    y: Math.Max(-n1 - half, -index - One) - Delta,
                    count: nodeCount,
                    max: AisleNodes(index));
            }

            // y direction
            for (var index = 0; index < 2 * Size; index++)
            {
                var nodeCount = hasNodeCounts
                    ? _nodeCounts[nodeCountIndex++]
                    : _sequence.Count(vertex => index == (vertex.y + Size) / 3);

                yield return (
                    group: "y",
                    index: index,
                    x: Math.Min(n1 + half, n2 - index) + Delta,
                    y: index - n1 + half,
                    count: nodeCount,
                    max: AisleNodes(index));
            }

            // z direction
            for (var index = 0; index < 2 * Size; index++)
            {
                var nodeCount = hasNodeCounts
                    ? _nodeCounts[nodeCountIndex++]
                    : _sequence.Count(vertex => index == (Size - vertex.x - vertex.y) / 3);

                yield return (
                    group: "z",
                    index: index,
                    x: Math.Max(-n1 - half, -index - One) - Delta,
                    y: Math.Min(n1 + half, n2 - index) + Delta,
                    count: nodeCount,
                    max: AisleNodes(index));
            }
        }

        private static bool TryCreateSequence(
            int size,
            int minSteps,
            IRandom random,
            out List<(int x, int y)> sequence)
        {
            sequence = [];

            var size3 = size * 3;
            var size6 = size * 6;
            var xrange = Enumerable.Range(1 - size3, size6);
            var yrange = Enumerable.Range(1 - size3, size6);

            static bool IsVertex((int x, int y) vertex)
            {
                return (vertex.x - vertex.y) % 3 == 0 && vertex.x % 3 != 0;
            }

            bool IsOnEdge((int x, int y) vertex)
            {
                return Math.Abs(vertex.x) == size3 - 1 ||
                    Math.Abs(vertex.y) == size3 - 1 ||
                    Math.Abs(vertex.x + vertex.y) == size3 - 1;
            }

            var vertexes = new HashSet<(int x, int y)>(
                xrange.SelectMany(x => yrange, (x, y) => (x, y))
                    .Where(vertex => Math.Abs(vertex.x + vertex.y) <= size3)
                    .Where(IsVertex));

            IEnumerable<(int x, int y)> AdjacentVertices((int x, int y) vertex)
            {
                if ((vertex.x + size3) % 3 == 1) // downward pointing triangle
                {
                    if (vertex.x > 1 - size3)
                    {
                        yield return (vertex.x - 2, vertex.y + 1);
                    }

                    if (vertex.y > 1 - size3)
                    {
                        yield return (vertex.x + 1, vertex.y - 2);
                    }

                    if (vertex.x + vertex.y < size3 - 1)
                    {
                        yield return (vertex.x + 1, vertex.y + 1);
                    }
                }
                else if ((vertex.x + size3) % 3 == 2) // upward pointing triangle
                {
                    if (vertex.x < size3 - 1)
                    {
                        yield return (vertex.x + 2, vertex.y - 1);
                    }

                    if (vertex.y < size3 - 1)
                    {
                        yield return (vertex.x - 1, vertex.y + 2);
                    }

                    if (vertex.x + vertex.y > 1 - size3)
                    {
                        yield return (vertex.x - 1, vertex.y - 1);
                    }
                }
            }

            var edges = vertexes.Where(IsOnEdge).ToList();
            var position = edges[random.Choose(edges.Count)];

            sequence.Add(position);
            vertexes.Remove(position);

            while (true)
            {
                var possibilities = AdjacentVertices(position).Where(vertex => vertexes.Contains(vertex)).ToList();
                if (possibilities.Count == 0)
                {
                    return IsOnEdge(position) && sequence.Count >= minSteps;
                }

                position = possibilities[random.Choose(possibilities.Count)];
                sequence.Add(position);
                vertexes.Remove(position);

                if (IsOnEdge(position) && random.Try(0.25))
                {
                    return sequence.Count >= minSteps;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int AisleNodes(int index) => index < Size
            ? 2 * (index + Size) + 1
            : 2 * (3 * Size - index) - 1;
    }
}
