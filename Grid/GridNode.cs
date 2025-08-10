using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public class GridNode : Node
    {
        public GridNode(int index, Coordinates coordinates)
            : base(index, coordinates)
        {
        }

        public void AddCounterIndexes(IEnumerable<int> indexes)
        {
            CounterIndexes = [.. indexes];
        }

        public IReadOnlyList<int> CounterIndexes { get; private set; }
    }
}
