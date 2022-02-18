using System.Collections.Generic;
using System.Linq;

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
            CounterIndexes = indexes.ToArray();
        }

        public IReadOnlyList<int> CounterIndexes { get; private set; }
    }
}
