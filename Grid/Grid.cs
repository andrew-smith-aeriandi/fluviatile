using Fluviatile.Grid.Random;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Fluviatile.Grid
{
    public class Grid : IGrid
    {
        private readonly IRandom _random;
        private readonly int _minSteps;

        private List<(int x, int y)> _sequence;

        public Grid(int size, double minStepsPercentage, IRandom random)
        {
            _minSteps = (int)(minStepsPercentage * size * size * 2);
            _random = random;

            Size = size;
            DisplayText = _random.Seed.ToString();
        }

        public int Size { get; }

        public string DisplayText { get; }

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

        public void SetSequence(IEnumerable<(int x, int y)> sequence)
        {
            Interlocked.Exchange(ref _sequence, [.. sequence]);
        }

        public void SetNodeCounts(IEnumerable<int> nodeCounts)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<(int x, int y)> Sequence()
        {
            return _sequence;
        }

        public IEnumerable<((float x, float y) from, (float x, float y) to)> GridLines()
        {
            const float tick = 0.3f;

            return Enumerable.Range(0, Size + 1).Select(y => ((-tick, (float)y), ((float)Size, (float)y)))
                .Concat(Enumerable.Range(0, Size + 1).Select(x => (((float)x, -tick), ((float)x, (float)Size))))
                .Concat(Enumerable.Range(0, Size).Select(y => ((0.0f, (float)y), ((float)Size - (float)y + tick, (float)Size + tick))))
                .Concat(Enumerable.Range(1, Size).Select(x => (((float)x, 0.0f), ((float)Size + tick, (float)Size - (float)x + tick))))
                .Append(((0.0f, (float)Size), (tick, (float)Size + tick)));
        }

        public IEnumerable<(string group, int index, float x, float y, int count, int max)> NodeCounts()
        {
            var third = 1.0f / 3.0f;
            var maxNodes = 2 * Size;

            for (var index = 0; index < Size; index++)
            {
                var xcount = _sequence.Count(vertex => index == vertex.x / 3);
                var xycount = _sequence.Count(vertex => index == (Size * 3 - vertex.y + vertex.x) / 3);

                yield return ("x", index, (float)index + third, -third, xcount, maxNodes);
                yield return ("z", index, (float)index + 2 * third, Size + third, xycount, maxNodes);
            }

            for (var index = 0; index < Size; index++)
            {
                var ycount = _sequence.Count(vertex => index == vertex.y / 3);
                var yxcount = _sequence.Count(vertex => index == (Size * 3 - vertex.x + vertex.y) / 3);

                yield return ("y", index, -third, (float)index + third, ycount, maxNodes);
                yield return ("z", index, (float)Size + third, (float)index + 2 * third, yxcount, maxNodes);
            }
        }

        private bool TryCreateSequence(int minSteps, out List<(int x, int y)> sequence)
        {
            sequence = [];

            var size3 = Size * 3;
            var xrange = Enumerable.Range(1, size3);
            var yrange = Enumerable.Range(1, size3);

            var vertexes = new HashSet<(int x, int y)>(
                xrange.SelectMany(x => yrange, (x, y) => (x, y)).Where(IsVertex));

            bool IsVertex((int x, int y) vertex)
            {
                return (vertex.x + vertex.y) % 3 == 0 && vertex.x % 3 != 0;
            }

            bool IsOnEdge((int x, int y) vertex)
            {
                return (vertex.x == 1 || vertex.x == size3 - 1 || vertex.y == 1 || vertex.y == size3 - 1);
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

                    if (vertex.y < size3 - 3)
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

                    if (vertex.x < size3 - 3)
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

        public void Dump()
        {
            foreach (var (x, y) in _sequence)
            {
                Debug.WriteLine($"({x}, {y})");
            }

            Debug.WriteLine($"Count: {_sequence.Count}");
        }

        public IEnumerable<IEnumerable<(float x, float y)>> GetMargins()
        {
            yield return Enumerable.Empty<(float x, float y)>();
        }

        public IEnumerable<((float x, float y) from, (float x, float y) to)> MarginLines()
        {
            return [];
        }

        public IEnumerable<((int x, int y) position, IEnumerable<(float x, float y)> polygon)> GridCells()
        {
            throw new NotImplementedException();
        }

        public void SetInitialState(IEnumerable<NodeState> state)
        {
            throw new NotImplementedException();
        }

        IEnumerable<NodeState> IGrid.GetInitialState()
        {
            throw new NotImplementedException();
        }
    }
}
