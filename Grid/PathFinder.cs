using Fluviatile.Grid.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fluviatile.Grid
{
    public class PathFinder
    {
        private readonly int _id;
        private readonly string _saveFilePath;
        private readonly Tableau _tableau;
        private readonly HashSet<TerminalNode> _endPoints;
        private readonly RouteLogFactory _routeLogFactory;
        private readonly RouteStateReaderWriter _routeStateReaderWriter;
        private readonly Dictionary<int[], int> _nodeCounts;
        private readonly int _threadCount;

        public PathFinder(Tableau tableau, int id, IEnumerable<TerminalNode> endPoints, int threadCount)
        {
            _id = id;
            _saveFilePath = Configuration.Filename(tableau.Shape, id);
            _tableau = tableau;
            _endPoints = new HashSet<TerminalNode>(endPoints);
            _routeLogFactory = new RouteLogFactory();
            _routeStateReaderWriter = new RouteStateReaderWriter();
            _nodeCounts = new Dictionary<int[], int>(new SequenceEqualEqualityComparer());
            _threadCount = threadCount;
        }

        public async Task<(int id, long routeCount, TimeSpan elapsedTime)> Explore(
            IEnumerable<Step> steps,
            TimeSpan monitorInterval,
            Progress progress,
            CancellationToken cancellationToken)
        {
            var routeCount = progress.RouteCount;
            var stopwatch = Stopwatch.StartNew();
            var routes = new BlockingCollection<Step>(new ConcurrentStack<Step>());

            foreach (var step in steps)
            {
                routes.Add(step);
            }

            var latch = new ManualResetEventSlim(initialState: true);
            Action action = () => Interlocked.Add(ref routeCount, StartWalk(routes, latch));

            while (routes.Any())
            {
                var tasks = Enumerable.Range(0, _threadCount)
                    .Select(index => Task.Run(action, cancellationToken))
                    .ToList();

                if (monitorInterval > TimeSpan.Zero)
                {
                    var persistTask = Task.Delay(monitorInterval, cancellationToken)
                        .ContinueWith(_ => latch.Reset(), TaskContinuationOptions.ExecuteSynchronously);

                    tasks.Add(persistTask);
                }

                await Task.WhenAll(tasks);

                var routeLog = _routeLogFactory.CreateLouteLog(
                    _tableau,
                    _endPoints,
                    routes.ToArray(),
                    new Progress(
                        routeCount,
                        progress.ElapsedTime + stopwatch.Elapsed));

                _routeStateReaderWriter.WriteToFile(routeLog, _saveFilePath);

                latch.Set();
            }

            stopwatch.Stop();




            return (_id, routeCount, progress.ElapsedTime + stopwatch.Elapsed);
        }

        private long StartWalk(BlockingCollection<Step> routes, ManualResetEventSlim latch)
        {
            var routeCount = 0L;

            while (latch.IsSet && routes.TryTake(out var step))
            {
                var left = step.TakeStep(Hexagon.Left);
                var isLeftAvailable = !step.TryFindNodeInRoute(left.Node, out var leftFootprint);

                var right = step.TakeStep(Hexagon.Right);
                var isRightAvailable = !step.TryFindNodeInRoute(right.Node, out var rightFootprint);

                if (isRightAvailable)
                {
                    if (right.Node is TerminalNode terminalNode)
                    {
                        if (_endPoints.Contains(terminalNode))
                        {
                            routeCount++;

                            var counts = _tableau.Shape.CountNodes(right);
                            _nodeCounts[counts] = _nodeCounts.TryGetValue(counts, out var value)
                                ? value + 1 : 1;
                        }
                    }
                    else if (isLeftAvailable || !_tableau.Shape.IsFullTurn(right, leftFootprint))
                    {
                        routes.Add(right);
                    }
                }

                if (isLeftAvailable)
                {
                    if (left.Node is TerminalNode terminalNode)
                    {
                        if (_endPoints.Contains(terminalNode))
                        {
                            routeCount++;

                            var counts = _tableau.Shape.CountNodes(left);
                            _nodeCounts[counts] = _nodeCounts.TryGetValue(counts, out var value)
                                ? value + 1 : 1;
                        }
                    }
                    else if (isRightAvailable || !_tableau.Shape.IsFullTurn(left, rightFootprint))
                    {
                        routes.Add(left);
                    }
                }
            }

            return routeCount;
        }
    }
}
