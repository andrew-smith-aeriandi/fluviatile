using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public class TerminalNode : Node
    {
        public TerminalNode(int index, Coordinates coordinates, IEnumerable<(Direction, Node)> links)
            : base(index, coordinates)
        {
            AddLinks(links);
        }
    }
}
