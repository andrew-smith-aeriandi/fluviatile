using Fluviatile.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Canvas
{
    public class RouteFinder
    {
        private readonly IRandom _random;
        private List<byte[]> _uniqueCounts;

        public RouteFinder(
            IRandom random,
            Shape shape)
        {
            if (shape is null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            Shape = shape;
            _random = random;
        }

        public async Task Initiate()
        {
            var uniqueCounts = await GetUniqueNodeCountsAsync(Shape);
            _uniqueCounts = uniqueCounts.ToList();
        }

        public Shape Shape { get; }

        public IReadOnlyList<int> SelectNodeCount()
        {
            var selection = _uniqueCounts[_random.Choose(_uniqueCounts.Count)];

            /*
            var selection = _uniqueCounts
                .OrderBy(x => x.Sum(y => (long)y))
                .First();
            */

            return selection.Select(b => (int)b).ToList();
        }

        private async Task<IEnumerable<byte[]>> GetUniqueNodeCountsAsync(Shape shape)
        {
            const int ThreadCount = 1;

            var tableau = shape.CreateTableau();

            var terminalNodePairs = tableau.TerminalNodeUniqueCombinations()
                .GroupBy(nodePair => (TerminalNode)nodePair.Node1)
                .Select(grp => (
                    startNode: grp.Key,
                    endNodes: grp.Select(node => (TerminalNode)node.Node2).ToList()))
                .ToList();

            var jobSpecs = terminalNodePairs.Select(nodePair =>
                new PathFinderJobSpec
                {
                    Tableau = tableau,
                    Name = nodePair.startNode.Index.ToString(),
                    StartPoint = nodePair.startNode,
                    EndPoints = nodePair.endNodes.ToList(),
                    ThreadCount = ThreadCount
                }).ToList();

            var combinedNodeCounts = new Dictionary<byte[], int>(new ByteSequenceEqualEqualityComparer());
            var tokenSource = new CancellationTokenSource();
            var tasks = new List<Task<(IDictionary<byte[], int>, PathFinderState)>>();

            foreach (var jobSpec in jobSpecs)
            {
                var pathFinderJob = new PathFinderJob(jobSpec);

                var pathFinderState = new PathFinderState
                {
                    Name = jobSpec.Name,
                    Steps = jobSpec.StartPoint.Links.Select(link => new Step(link.Value, link.Key)).ToList(),
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
