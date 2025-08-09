using Fluviatile.Grid.Random;
using Fluviatile.Grid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fluviatile.Grid
{
    public class RouteFinder
    {
        private readonly IRandom _random;
        private IReadOnlyList<byte[]> _uniqueCounts;

        public RouteFinder(
            IRandom random,
            Shape shape)
        {
            ArgumentNullException.ThrowIfNull(shape);

            Shape = shape;
            _random = random;
        }

        public async Task Initiate(string filePath)
        {
            var uniqueCounts = UniqueNodeCountsSerializer.Load(filePath);
            if (uniqueCounts is null)
            {
                uniqueCounts = [.. await GetUniqueNodeCountsAsync(Shape)];
                UniqueNodeCountsSerializer.Save(filePath, uniqueCounts);
            }

            Interlocked.Exchange(ref _uniqueCounts, uniqueCounts);
        }

        public Shape Shape { get; }

        public IReadOnlyList<byte[]> UniqueCounts => _uniqueCounts;

        public IReadOnlyList<int> SelectRandomNodeCount()
        {
            var selection = _uniqueCounts is not null
                ? _uniqueCounts[_random.Choose(_uniqueCounts.Count)]
                : default;

            return selection?.Select(b => (int)b).ToList();
        }

        public IReadOnlyList<int> SelectRandomNodeCount(Func<byte[], bool> predicate)
        {
            var selection = _uniqueCounts.Where(predicate).RandomElement(_random);
            return selection?.Select(b => (int)b).ToList();
        }

        private static async Task<IEnumerable<byte[]>> GetUniqueNodeCountsAsync(Shape shape)
        {
            const int ThreadCount = 1;

            var tableau = shape.CreateTableau();

            var terminalNodePairs = tableau.TerminalNodeUniqueCombinations()
                .GroupBy(nodePair => (TerminalNode)nodePair.Node1)
                .Select(grp => (
                    startNode: grp.Key,
                    endNodes: grp.Select(node => (TerminalNode)node.Node2).ToList()))
                .ToList();

            var jobSpecs = terminalNodePairs
                .Select(nodePair =>
                    new PathFinderJobSpec
                    {
                        Tableau = tableau,
                        Name = nodePair.startNode.Index.ToString(),
                        StartPoint = nodePair.startNode,
                        EndPoints = [.. nodePair.endNodes],
                        ThreadCount = ThreadCount
                    })
                .ToList();

            var combinedNodeCounts = new Dictionary<byte[], int>(new ByteSequenceEqualEqualityComparer());
            var tokenSource = new CancellationTokenSource();
            var tasks = new List<Task<(IDictionary<byte[], int>, PathFinderState)>>();

            foreach (var jobSpec in jobSpecs)
            {
                var pathFinderJob = new PathFinderJob(jobSpec);
                var pathFinderState = new PathFinderState
                {
                    Name = jobSpec.Name,
                    Steps = [.. jobSpec.StartPoint.Links.Select(link => new Step(link.Value, link.Key))],
                    Progress = new Progress()
                };

                tasks.Add(pathFinderJob.ExploreAsync(pathFinderState, tokenSource.Token));
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                var (nodeCounts, pathFinderState) = task.Result;

                foreach (var nodeCount in nodeCounts)
                {
                    if (!combinedNodeCounts.TryAdd(nodeCount.Key, nodeCount.Value))
                    {
                        combinedNodeCounts[nodeCount.Key] += nodeCount.Value;
                    }
                }
            }

            return combinedNodeCounts
                .Where(kv => kv.Value == 1)
                .Select(kv => kv.Key);
        }
    }
}
