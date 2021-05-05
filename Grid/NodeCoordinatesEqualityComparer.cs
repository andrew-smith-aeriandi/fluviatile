using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public class NodeCoordinatesEqualityComparer : IEqualityComparer<Node>
    {
        public static readonly NodeCoordinatesEqualityComparer Default = new NodeCoordinatesEqualityComparer();

        public bool Equals(Node x, Node y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Coordinates.Equals(y.Coordinates);
        }

        public int GetHashCode(Node obj)
        {
            return obj.Coordinates.GetHashCode();
        }
    }
}
