using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluviatile.Grid
{
    public class Node
    {
        public Node(int index, Coordinates coordinates)
        {
            Index = index;
            Coordinates = coordinates;
        }

        public int Index { get; }

        public Coordinates Coordinates { get; }

        public IDictionary<Direction, Node> Links { get; private set; }

        public void AddLinks(IEnumerable<(Direction direction, Node node)> links)
        {
            if (Links != null)
            {
                throw new InvalidOperationException();
            }

            if (links == null)
            {
                throw new ArgumentNullException(nameof(links));
            }

            Links = links.ToDictionary(
                item => item.direction,
                item => item.node);
        }

        public override string ToString()
        {
            return Coordinates.ToString();
        }
    }
}
