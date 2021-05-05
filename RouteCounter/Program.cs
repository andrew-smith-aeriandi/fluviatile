using Fluviatile.Grid;
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
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            var tableauReaderWriter = new RouteStateReaderWriter();

            if (!Directory.Exists(Configuration.FolderName))
            {
                Directory.CreateDirectory(Configuration.FolderName);
            }

            var shape = GetShape(args);
            var tableau = shape.CreateTableau();
            var concurrency = GetConcurrency(args);
            var persistInterval = GetPersistInterval(args);

            var terminalNodePairs = tableau
                .TerminalNodeUniqueCombinations()
                .GroupBy(item => (TerminalNode)item.Node1)
                .Select(grp => (startNode: grp.Key, endNodes: grp.Select(node => (TerminalNode)node.Node2).ToList()))
                .ToList();

            foreach (var nodePair in terminalNodePairs)
            {
                Trace.WriteLine($"{nodePair.startNode.Index}: {string.Join(", ", nodePair.endNodes.Select(node => node.Index))}");
            }

            var jobs = new List<(int id, List<Step> steps, List<TerminalNode> endPoints, Progress progress)>();

            foreach (var nodePair in terminalNodePairs)
            {
                var jobId = nodePair.startNode.Index;
                var saveFilePath = Configuration.Filename(shape, jobId);
                if (!File.Exists(saveFilePath))
                {
                    jobs.Add((
                        jobId,
                        nodePair.startNode.Links.Select(link => new Step(link.Value, link.Key)).ToList(),
                        nodePair.endNodes.ToList(),
                        new Progress(0L, TimeSpan.Zero)));
                }
                else
                {
                    var savedState = tableauReaderWriter.ReadFromFile(saveFilePath);

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
                            tableau.Nodes[item.Position],
                            new Direction(item.Direction));

                        queue.Enqueue((step, item.Id));
                    }

                    while (queue.Any())
                    {
                        var (previousStep, id) = queue.Dequeue();
                        items = savedState.Steps.Where(x => x.PreviousId == id).ToList();

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
                                tableau.Nodes[item.Position],
                                direction,
                                twist,
                                previousStep);

                            queue.Enqueue((step, item.Id));
                        }
                    }

                    jobs.Add((
                        jobId,
                        steps,
                        endPoints,
                        savedState.Progress));
                }
            }

            var tokenSource = new CancellationTokenSource();
            var tasks = new List<Task<(int id, long routeCount, TimeSpan elapsedTime)>>();

            foreach (var job in jobs)
            {
                var pathFinder = new PathFinder(tableau, job.id, job.endPoints, concurrency);
                tasks.Add(pathFinder.Explore(job.steps, persistInterval, job.progress, tokenSource.Token));
            }

            await Task.WhenAll(tasks);

            var combinedElapsedTime = TimeSpan.Zero;
            var combinedRouteCount = 0L;

            foreach (var task in tasks)
            {
                var (id, routeCount, elapsedTime) = task.Result;

                combinedRouteCount += routeCount;
                if (elapsedTime > combinedElapsedTime)
                {
                    combinedElapsedTime = elapsedTime;
                }

                Trace.WriteLine($"Result for job {id}: routes = {routeCount}, elapsed time = {elapsedTime}");
            }

            Trace.WriteLine($"Overall result: routes = {combinedRouteCount}, elapsed time = {combinedElapsedTime}");
        }

        private static Shape GetShape(string[] args)
        {
            var size = int.Parse(
                args.Select(arg => Regex.Match(arg, "^-([1-9][0-9]*)$"))
                    .FirstOrDefault(m => m.Success)
                    ?.Groups[1].Value ?? "1");

            if (args.Length == 0)
            {
                return new Hexagon(size);
            }

            switch (args[0].ToLowerInvariant())
            {
                case "hexagon":
                    return new Hexagon(size);

                default:
                    throw new NotSupportedException($"Shape '{args[0]}' not currently supported.");
            }
        }

        private static int GetConcurrency(string[] args)
        {
            return int.Parse(
                args.Select(arg => Regex.Match(arg, "^-[cC]([1-9][0-9]*)$"))
                    .FirstOrDefault(m => m.Success)
                    ?.Groups[1].Value ?? "1");
        }

        private static TimeSpan GetPersistInterval(string[] args)
        {
            return TimeSpan.FromSeconds(double.Parse(
                args.Select(arg => Regex.Match(arg, @"^-[pP]([0-9]+(\.[0-9]+)?)$"))
                    .FirstOrDefault(m => m.Success)
                    ?.Groups[1].Value ?? "10"));
        }
    }
}
