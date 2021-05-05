//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;

//namespace WindowsFormsApp2
//{
//    public class HexGrid
//    {
//        private readonly int _side;
//        private readonly int _side3;
//        private readonly int _edges;

//        private readonly IRandom _random;
//        private readonly List<(int x, int y)> _sequence;

//        public HexGrid(int side, int minSteps, IRandom random)
//        {
//            _side = side;
//            _edges = 6 * side;

//            _side3 = side * 3;

//            _random = random;

//            while (true)
//            {
//                _sequence = new List<(int x, int y)>();

//                if (Create() && _sequence.Count >= minSteps)
//                {
//                    return;
//                }
//            }
//        }

//        public int Seed => _random.Seed;

//        public int Side => _side;

//        public List<(int x, int y)> Sequence => _sequence;

//        public IEnumerable<((float x, float y) from, (float x, float y) to)> GridLines()
//        {
//            const float tick = 0.3f;

//            return Enumerable.Range(0, _side + 1).Select(u => 
//                    ((-tick, (float)u), ((float)(_side + u), (float)u)))
//                .Concat(Enumerable.Range(0, _side + 1).Select(u =>
//                    (((float)(_side - u) - tick, -tick), ((float)(2 * _side), (float)(_side + u)))))
//                .Concat(Enumerable.Range(0, _side + 1).Select(u =>
//                    (((float)(2 * _side - u), (float)(_side - u) -tick), ((float)(2 * _side - u), (float)(2 * _side)))))
//                .Concat(Enumerable.Range(0, _side + 1).Select(u =>
//                    (((float)(2 * _side) + tick, (float)(2 * _side - u)), ((float)(2 * _side - u), (float)(2 * _side - u)))))
//                .Concat(Enumerable.Range(0, _side + 1).Select(u => 
//                    (((float)(_side + u) + tick, (float)(2 * _side) + tick), (0.0f, (float)(_side - u)))))
//                .Concat(Enumerable.Range(0, _side + 1).Select(u => 
//                    (((float)u, (float)(_side + u) + tick), ((float)u, 0.0f))));
//        }

//        public IEnumerable<(int x, int count)> XCounts()
//        {
//            return Enumerable.Range(0, 2 * _side).Select(x => 
//                (x, _sequence.Count(vertex => x == vertex.x / 3)));
//        }

//        public IEnumerable<(int y, int count)> YCounts()
//        {
//            return Enumerable.Range(0, 2 * _side).Select(y =>
//                (y, _sequence.Count(vertex => y == vertex.y / 3)));
//        }

//        public IEnumerable<(int y, int count)> XYCounts()
//        {
//            return Enumerable.Range(0, 2 * _side).Select(xy =>
//                (xy, _sequence.Count(vertex => y == vertex.y / 3)));
//        }



//        public IEnumerable<(int x, int county)> XCounts()
//        {
//            return Enumerable.Range(0, _side)
//                .Select(index => (
//                    index,
//                    _sequence.Count(vertex => index == vertex.x / 3),
//                    _sequence.Count(vertex => index == (_width3 - vertex.y + vertex.x) / 3)));
//        }

//        public IEnumerable<(int y, int countx, int countxy)> YCounts()
//        {
//            return Enumerable.Range(0, _height)
//                .Select(index => (
//                    index,
//                    _sequence.Count(vertex => index == vertex.y / 3),
//                    _sequence.Count(vertex => index == (_height3 - vertex.x + vertex.y) / 3)));
//        }

//        private bool Create()
//        {
//            var xrange = Enumerable.Range(1, _side3 * 2);
//            var yrange = Enumerable.Range(1, _side3 * 2);

//            var vertexes = new HashSet<(int x, int y)>(
//                xrange.SelectMany(x => yrange, (x, y) => (x: x, y: y)).Where(IsVertex));

//            bool IsVertex((int x, int y) vertex)
//            {
//                return (vertex.x + vertex.y) % 3 == 0 &&
//                       vertex.x % 3 != 0 &&
//                       vertex.y > vertex.x - _side3 &&
//                       vertex.y < vertex.x + _side3;
//            }

//            bool IsOnEdge((int x, int y) vertex)
//            {
//                return vertex.x == 1 ||
//                       vertex.y - vertex.x == _side3 - 1 ||
//                       vertex.y == _side3 * 2 - 1 ||
//                       vertex.x == _side3 * 2 - 1 ||
//                       vertex.x - vertex.y == _side3 - 1 ||
//                       vertex.y == 1;
//            }

//            IEnumerable<(int x, int y)> AdjacentVertices((int x, int y) vertex)
//            {
//                if (vertex.x % 3 == 1)
//                {
//                    if (vertex.y > vertex.x - _side3 + 3)
//                    {
//                        yield return (vertex.x + 1, vertex.y - 1);
//                    }

//                    if (vertex.x > 3)
//                    {
//                        yield return (vertex.x - 2, vertex.y - 1);
//                    }

//                    if (vertex.y < _side3 * 2 - 3)
//                    {
//                        yield return (vertex.x + 1, vertex.y + 2);
//                    }
//                }
//                else if (vertex.x % 3 == 2)
//                {
//                    if (vertex.y < vertex.x + _side3 - 3)
//                    {
//                        yield return (vertex.x - 1, vertex.y + 1);
//                    }

//                    if (vertex.y > 3)
//                    {
//                        yield return (vertex.x - 1, vertex.y - 2);
//                    }

//                    if (vertex.x < _side3 - 3)
//                    {
//                        yield return (vertex.x + 2, vertex.y + 1);
//                    }
//                }
//            }

//            var edges = vertexes.Where(IsOnEdge).ToList();
//            var position = edges[_random.Choose(edges.Count)];

//            _sequence.Add(position);
//            vertexes.Remove(position);

//            while (true)
//            {
//                var possibilities = AdjacentVertices(position).Where(vertex => vertexes.Contains(vertex)).ToList();
//                if (possibilities.Count == 0)
//                {
//                    return IsOnEdge(position);
//                }

//                position = possibilities[_random.Choose(possibilities.Count)];
//                _sequence.Add(position);
//                vertexes.Remove(position);

//                if (IsOnEdge(position) && _random.Try(0.25))
//                {
//                    return true;
//                }
//            }
//        }

//        public void Dump()
//        {
//            foreach (var (x, y) in _sequence)
//            {
//                Debug.WriteLine($"({x}, {y})");
//            }

//            Debug.WriteLine($"Count: {_sequence.Count}");

//            foreach (var (i, c1, c2) in XCounts())
//            {
//                Debug.WriteLine($"({i}, {c1}, {c2})");
//            }

//            foreach (var (i, c1, c2) in YCounts())
//            {
//                Debug.WriteLine($"({i}, {c1}, {c2})");
//            }
//        }

//    }
//}
