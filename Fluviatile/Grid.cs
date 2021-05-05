using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WindowsFormsApp2
{
    public class Grid
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _width3;
        private readonly int _height3;
        private readonly int _edges;

        private readonly IRandom _random;
        private readonly List<(int x, int y)> _sequence;

        public Grid(int width, int height, int minSteps, IRandom random)
        {
            _width = width;
            _height = height;
            _edges = 2 * (width + height);

            _width3 = width * 3;
            _height3 = height * 3;

            _random = random;

            while (true)
            {
                _sequence = new List<(int x, int y)>();

                if (Create() && _sequence.Count >= minSteps)
                {
                    return;
                }
            }
        }

        public int Seed => _random.Seed;

        public int Width => _width;

        public int Height => _height;

        public List<(int x, int y)> Sequence => _sequence;

        public IEnumerable<((float x, float y) from, (float x, float y) to)> GridLines()
        {
            const float tick = 0.3f;

            return Enumerable.Range(0, _height + 1).Select(y => ((-tick, (float)y), ((float)_width, (float)y)))
                .Concat(Enumerable.Range(0, _width + 1).Select(x => (((float)x, -tick), ((float)x, (float)_height))))
                .Concat(Enumerable.Range(0, _height).Select(y => ((0.0f, (float)y), ((float)_height - (float)y + tick, (float)_height + tick))))
                .Concat(Enumerable.Range(1, _width).Select(x => (((float)x, 0.0f), ((float)_width + tick, (float)_width - (float)x + tick))))
                .Append(((0.0f, (float)_height), (tick, (float)_height + tick)));
        }

        public IEnumerable<(int x, int county, int countxy)> XCounts()
        {
            return Enumerable.Range(0, _width)
                .Select(index => (
                    index,
                    _sequence.Count(vertex => index == vertex.x / 3),
                    _sequence.Count(vertex => index == (_width3 - vertex.y + vertex.x) / 3)));
        }

        public IEnumerable<(int y, int countx, int countxy)> YCounts()
        {
            return Enumerable.Range(0, _height)
                .Select(index => (
                    index,
                    _sequence.Count(vertex => index == vertex.y / 3),
                    _sequence.Count(vertex => index == (_height3 - vertex.x + vertex.y) / 3)));
        }

        private bool Create()
        {
            var xrange = Enumerable.Range(1, _width3);
            var yrange = Enumerable.Range(1, _height3);

            var vertexes = new HashSet<(int x, int y)>(
                xrange.SelectMany(x => yrange, (x, y) => (x: x, y: y)).Where(IsVertex));

            bool IsVertex((int x, int y) vertex)
            {
                return (vertex.x + vertex.y) % 3 == 0 && vertex.x % 3 != 0;
            }

            bool IsOnEdge((int x, int y) vertex)
            {
                return (vertex.x == 1 || vertex.x == _width3 - 1 || vertex.y == 1 || vertex.y == _height3 - 1);
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

                    if (vertex.y < _height3 - 3)
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

                    if (vertex.x < _width3 - 3)
                    {
                        yield return (vertex.x + 2, vertex.y + 1);
                    }
                }
            }

            var edges = vertexes.Where(IsOnEdge).ToList();
            var position = edges[_random.Choose(edges.Count)];

            _sequence.Add(position);
            vertexes.Remove(position);

            while (true)
            {
                var possibilities = AdjacentVertices(position).Where(vertex => vertexes.Contains(vertex)).ToList();
                if (possibilities.Count == 0)
                {
                    return IsOnEdge(position);
                }

                position = possibilities[_random.Choose(possibilities.Count)];
                _sequence.Add(position);
                vertexes.Remove(position);

                if (IsOnEdge(position) && _random.Try(0.25))
                {
                    return true;
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

            foreach (var (i, c1, c2) in XCounts())
            {
                Debug.WriteLine($"({i}, {c1}, {c2})");
            }

            foreach (var (i, c1, c2) in YCounts())
            {
                Debug.WriteLine($"({i}, {c1}, {c2})");
            }
        }

    }
}
