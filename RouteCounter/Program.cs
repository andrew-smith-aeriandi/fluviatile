using Fluviatile.Grid;
using Fluviatile.Grid.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RouteCounter
{
    public partial class Program
    {
        private readonly static Regex SizeArgPattern = SizeArgRegex();
        private readonly static Regex ConcurrencyArgPattern = ConcurrencyArgRegex();
        private readonly static Regex PersistIntervalArgPattern = PersistIntervalArgRegex();

        public static async Task Main(string[] args)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            if (!Directory.Exists(Configuration.FolderName))
            {
                Directory.CreateDirectory(Configuration.FolderName);
            }

            var shape = GetShape(args);
            var tableau = shape.CreateTableau();
            var concurrency = GetConcurrency(args);
            var persistInterval = GetPersistInterval(args);

            var terminalNodePairs = tableau.TerminalNodeUniqueCombinations()
                .GroupBy(nodePair => (TerminalNode)nodePair.Node1)
                .Select(grp => (
                    startNode: grp.Key,
                    endNodes: grp.Select(node => (TerminalNode)node.Node2).ToList()))
                .ToList();

            foreach (var (startNode, endNodes) in terminalNodePairs)
            {
                Trace.WriteLine($"{startNode.Index}: {string.Join(", ", endNodes.Select(node => node.Index))}");
            }

            var jobs = new List<(PathFinderJob, PathFinderState)>();

            foreach (var (startNode, endNodes) in terminalNodePairs)
            {
                var jobAdded = false;
                var jobSpec = new PathFinderJobSpec
                {
                    Name = startNode.Index.ToString(),
                    Tableau = tableau,
                    StartPoint = startNode,
                    EndPoints = endNodes,
                    ThreadCount = concurrency,
                    MonitorInterval = persistInterval
                };

                if (persistInterval > TimeSpan.Zero)
                {
                    var saveFilePath = Configuration.RoutesFilename(shape, jobSpec.Name);
                    Trace.WriteLine($"Save file path: {saveFilePath}");

                    if (File.Exists(saveFilePath))
                    {
                        var savedState = RouteStateReaderWriter.ReadFromFile(saveFilePath);

                        if (savedState.Shape != shape.Name)
                        {
                            throw new Exception($"The shape {savedState.Shape} specfied in file '{saveFilePath}' does not match the expected value {shape.Name}.");
                        }

                        if (savedState.Size != shape.Size)
                        {
                            throw new Exception($"The size {savedState.Size} specfied in file '{saveFilePath}' does not match the expected value {shape.Size}.");
                        }

                        var endPoints = savedState.TerminalNodes
                            .Select(index => tableau.TerminalNodes[index])
                            .ToList();

                        var steps = new List<Step>();
                        var queue = new Queue<(Step step, int id)>();
                        var items = savedState.Steps.Where(x => x.PreviousId == 0).ToList();

                        foreach (var item in items)
                        {
                            var step = new Step(
                                node: tableau.Nodes[item.Position],
                                direction: new Direction(item.Direction));

                            queue.Enqueue((step, item.Id));
                        }

                        while (queue.Count > 0)
                        {
                            var (previousStep, id) = queue.Dequeue();
                            items = [.. savedState.Steps.Where(x => x.PreviousId == id)];

                            if (items.Count == 0)
                            {
                                steps.Add(previousStep);
                                continue;
                            }

                            foreach (var item in items)
                            {
                                var direction = new Direction(item.Direction);
                                var twist = direction - previousStep.Direction;

                                var step = new Step(
                                    node: tableau.Nodes[item.Position],
                                    direction: direction,
                                    twist: twist,
                                    previous: previousStep);

                                queue.Enqueue((step, item.Id));
                            }
                        }

                        var pathFinderJob = new PathFinderJob(jobSpec); //TODO: paths
                        var pathFinderState = new PathFinderState
                        {
                            Name = jobSpec.Name,
                            Steps = steps,
                            Progress = savedState.Progress
                        };

                        jobs.Add((pathFinderJob, pathFinderState));
                        jobAdded = true;
                    }
                }

                if (!jobAdded)
                {
                    var pathFinderJob = new PathFinderJob(jobSpec);
                    var pathFinderState = new PathFinderState
                    {
                        Name = jobSpec.Name,
                        Steps = [.. startNode.Links.Select(link => new Step(link.Value, link.Key))],
                        Progress = new Progress()
                    };

                    jobs.Add((pathFinderJob, pathFinderState));
                }
            }

            var tokenSource = new CancellationTokenSource();
            var tasks = new List<Task<(IDictionary<byte[], int>, PathFinderState)>>();

            foreach (var (pathFinderJob, pathFinderState) in jobs)
            {
                tasks.Add(pathFinderJob.ExploreAsync(pathFinderState, tokenSource.Token));
            }

            await Task.WhenAll(tasks);

            var combinedElapsedTime = TimeSpan.Zero;
            var combinedRouteCount = 0L;

            var combinedNodeCounts = new Dictionary<byte[], int>(new ByteSequenceEqualEqualityComparer());

            foreach (var task in tasks)
            {
                var (nodeCounts, pathFinderState) = task.Result;

                foreach (var nodeCount in nodeCounts)
                {
                    combinedNodeCounts.TryAdd(nodeCount.Key, 0);
                    combinedNodeCounts[nodeCount.Key] += nodeCount.Value;
                }

                var uniqueSolutionCount = nodeCounts.Count(item => item.Value == 1);

                combinedRouteCount += pathFinderState.Progress.RouteCount;
                if (pathFinderState.Progress.ElapsedTime > combinedElapsedTime)
                {
                    combinedElapsedTime = pathFinderState.Progress.ElapsedTime;
                }

                Trace.WriteLine($"Result for job {pathFinderState.Name}: routes = {pathFinderState.Progress.RouteCount}, distinct solutions = {nodeCounts.Count}, unique solutions = {uniqueSolutionCount}, elapsed time = {pathFinderState.Progress.ElapsedTime}");
            }

            var combinedUniqueSolutionCount = combinedNodeCounts.Count(item => item.Value == 1);

            Trace.WriteLine($"Overall result: routes = {combinedRouteCount}, distinct solutions = {combinedNodeCounts.Count}, unique solutions = {combinedUniqueSolutionCount}, elapsed time = {combinedElapsedTime}");
        }

        private static Hexagon GetShape(string[] args)
        {
            var sizeArgMatch = args
                .Select(arg => SizeArgPattern.Match(arg))
                .FirstOrDefault(m => m.Success);

            var size = sizeArgMatch is not null
                ? int.Parse(sizeArgMatch.Groups[1].Value)
                : 1;

            if (args.Length == 0)
            {
                return new Hexagon(size);
            }

            return args[0].ToLowerInvariant() switch
            {
                "hexagon" => new Hexagon(size),
                _ => throw new NotSupportedException($"Shape '{args[0]}' not currently supported."),
            };
        }

        private static int GetConcurrency(string[] args)
        {
            var concurrencyArgMatch = args
                .Select(arg => ConcurrencyArgPattern.Match(arg))
                .FirstOrDefault(m => m.Success);

            return concurrencyArgMatch is not null
                ? int.Parse(concurrencyArgMatch.Groups[1].Value)
                : 1;
        }

        private static TimeSpan GetPersistInterval(string[] args)
        {
            var persistIntervalArgMatch = args
                .Select(arg => PersistIntervalArgPattern.Match(arg))
                .FirstOrDefault(m => m.Success);

            return persistIntervalArgMatch is not null
                ? TimeSpan.FromSeconds(double.Parse(persistIntervalArgMatch.Groups[1].Value))
                : TimeSpan.Zero;
        }

        [GeneratedRegex("^-([1-9][0-9]*)$")]
        private static partial Regex SizeArgRegex();

        [GeneratedRegex("^-[cC]([1-9][0-9]*)$")]
        private static partial Regex ConcurrencyArgRegex();

        [GeneratedRegex(@"^-[pP]([0-9]+(\.[0-9]+)?)$")]
        private static partial Regex PersistIntervalArgRegex();
    }
}
