using ConcurrentCollections;
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
    public class PathFinderJob
    {
        private readonly string _name;
        private readonly Tableau _tableau;
        private readonly TerminalNode _startPoint;
        private readonly HashSet<TerminalNode> _endPoints;
        private readonly int _threadCount;
        private readonly TimeSpan _monitorInterval;
        private readonly string _saveFilePath;

        private readonly IReadOnlyDictionary<TerminalNode, Func<Step, IEnumerable<byte[]>>> _equivalentPathsLookup;
        private readonly ConcurrentDictionary<byte[], int> _nodeCounts;
        private readonly ConcurrentHashSet<byte[]> _paths;

        public PathFinderJob(
            PathFinderJobSpec spec,
            IEnumerable<byte[]> paths = null,
            IDictionary<byte[], int> nodeCounts = null)
        {
            _name = spec.Name;
            _tableau = spec.Tableau;
            _startPoint = spec.StartPoint;
            _endPoints = new HashSet<TerminalNode>(spec.EndPoints);
            _threadCount = spec.ThreadCount;
            _monitorInterval = spec.MonitorInterval;
            _saveFilePath = Configuration.RoutesFilename(_tableau.Shape, _name);

            _equivalentPathsLookup = _endPoints.ToDictionary(
                endPoint => endPoint,
                endPoint => _tableau.Shape.GetEquivalentPathsDelegate(new NodePair(_startPoint, endPoint)));

            _paths = paths is not null
                ? new ConcurrentHashSet<byte[]>(paths, new ByteArrayEqualityComparer())
                : new ConcurrentHashSet<byte[]>(new ByteArrayEqualityComparer());

            _nodeCounts = nodeCounts is not null
                ? new ConcurrentDictionary<byte[], int>(nodeCounts, new ByteSequenceEqualEqualityComparer())
                : new ConcurrentDictionary<byte[], int>(new ByteSequenceEqualEqualityComparer());
        }

        public string Name => _name;

        public IEnumerable<byte[]> Paths => _paths;

        public IDictionary<byte[], int> NodeCounts => _nodeCounts;

        public async Task<(IDictionary<byte[], int> nodeCounts, PathFinderState state)> ExploreAsync(
            PathFinderState state,
            CancellationToken cancellationToken)
        {
            var routeCount = state.Progress.RouteCount;
            var stopwatch = Stopwatch.StartNew();
            var routes = new BlockingCollection<Step>(new ConcurrentStack<Step>());

            foreach (var step in state.Steps)
            {
                routes.Add(step);
            }

            var latch = new ManualResetEventSlim(initialState: true);
            void StartWalkAction() => Interlocked.Add(ref routeCount, StartWalk(routes, latch));

            while (routes.Any())
            {
                var tasks = Enumerable.Range(0, _threadCount)
                    .Select(index => Task.Run(StartWalkAction, cancellationToken))
                    .ToList();

                if (_monitorInterval > TimeSpan.Zero)
                {
                    var persistTask = Task.Delay(_monitorInterval, cancellationToken)
                        .ContinueWith(_ => latch.Reset(), TaskContinuationOptions.ExecuteSynchronously);

                    tasks.Add(persistTask);
                }

                await Task.WhenAll(tasks);

                if (_monitorInterval > TimeSpan.Zero)
                {
                    //TODO: export paths
                    var routeLog = RouteLogFactory.CreateRouteLog(
                        _tableau,
                        _endPoints,
                        routes.ToArray(),
                        new Progress
                        {
                            RouteCount = routeCount,
                            ElapsedTime = state.Progress.ElapsedTime + stopwatch.Elapsed
                        });

                    RouteStateReaderWriter.WriteToFile(routeLog, _saveFilePath);
                }

                latch.Set();
            }

            stopwatch.Stop();


            return (
                nodeCounts: _nodeCounts,
                state: state with
                {
                    Steps = routes.ToList(),
                    Progress = new Progress
                    {
                        RouteCount = routeCount,
                        ElapsedTime = state.Progress.ElapsedTime + stopwatch.Elapsed
                    }
                });
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
                            //routeCount++;
                            //RecordNodeCount(right);

                            if (IsRouteUnrecorded(_equivalentPathsLookup[terminalNode].Invoke(right)))
                            {
                                routeCount++;
                                RecordNodeCount(right);
                            }
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
                            //routeCount++;
                            //RecordNodeCount(left);

                            if (IsRouteUnrecorded(_equivalentPathsLookup[terminalNode].Invoke(left)))
                            {
                                routeCount++;
                                RecordNodeCount(left);
                            }
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

        private bool IsRouteUnrecorded(IEnumerable<byte[]> equivalentPaths)
        {
            var pathUnrecorded = true;

            foreach (var path in equivalentPaths)
            {
                if (!_paths.Add(path))
                {
                    pathUnrecorded = false;
                }
            }

            return pathUnrecorded;
        }

        private void RecordNodeCount(Step lastStep)
        {
            var counts = _tableau.Shape.CountNodesAndNormalise(lastStep, _tableau.Shape.NodeCountPermutations);
            _nodeCounts[counts] = _nodeCounts.TryGetValue(counts, out var value)
                ? value + 1
                : 1;
        }
    }
}
