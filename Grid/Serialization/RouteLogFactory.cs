using System.Collections.Generic;
using System.Linq;

namespace Fluviatile.Grid.Serialization
{
    public class RouteLogFactory
    {
        public RouteLog CreateLouteLog(
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
                TerminalNodes = GetTerminalNodeIndices(endPoints).ToList(),
                Steps = GetDistinctSteps(routes).ToList()
            };
        }

        private IEnumerable<int> GetTerminalNodeIndices(IEnumerable<TerminalNode> endPoints)
        {
            return endPoints
                .Select(node => node.Index);
        }

        private IEnumerable<Footprint> GetDistinctSteps(IEnumerable<Step> routes)
        {
            var stack = new Stack<Step>(routes);
            var distinctSteps = new Dictionary<Step, int>();
            var stepIdCounter = 0;

            while (stack.Any())
            {
                var step = stack.Pop();
                if (distinctSteps.ContainsKey(step))
                {
                    continue;
                }

                stepIdCounter++;
                distinctSteps.Add(step, stepIdCounter);

                if (step.Previous != null)
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
