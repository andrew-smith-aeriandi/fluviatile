using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public class TerminalNode : Node
    {
        public TerminalNode(int index, Coordinates coordinates, IEnumerable<(Direction, Node)> links, Aisle aisle)
            : base(index, coordinates)
        {
            Aisle = aisle;
            AddLinks(links);
        }

        public Aisle Aisle { get; }
    }
}
