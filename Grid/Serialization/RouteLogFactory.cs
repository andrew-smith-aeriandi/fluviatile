using System.Collections.Generic;
using System.Linq;

namespace Fluviatile.Grid.Serialization
{
    public static class RouteLogFactory
    {
        public static RouteLog CreateRouteLog(
            Tableau tableau,
            IEnumerable<TerminalNode> endPoints,
            IEnumerable<Step> routes,
            Progress progress)
        {
            return new RouteLog
            {
                Shape = tableau.Shape.Name,
                Size = tableau.Shape.Size,
                Progress = progress,
                TerminalNodes = [.. GetTerminalNodeIndices(endPoints)],
                Steps = [.. GetDistinctSteps(routes)]
            };
        }

        private static IEnumerable<int> GetTerminalNodeIndices(IEnumerable<TerminalNode> endPoints)
        {
            return endPoints.Select(node => node.Index);
        }

        private static IEnumerable<Footprint> GetDistinctSteps(IEnumerable<Step> routes)
        {
            var stack = new Stack<Step>(routes);
            var distinctSteps = new Dictionary<Step, int>();
            var stepIdCounter = 0;

            while (stack.Count > 0)
            {
                var step = stack.Pop();
                if (distinctSteps.ContainsKey(step))
                {
                    continue;
                }

                stepIdCounter++;
                distinctSteps.Add(step, stepIdCounter);

                if (step.Previous is not null)
                {
                    stack.Push(step.Previous);
                }
            }

            return distinctSteps
                .OrderBy(kv => kv.Key.Count)
                .Select(kv => new Footprint
                {
                    Id = kv.Value,
                    PreviousId = kv.Key.Previous != null ? distinctSteps[kv.Key.Previous] : 0,
                    Position = kv.Key.Node.Index,
                    Direction = kv.Key.Direction.Value
                });
        }
    }
}
