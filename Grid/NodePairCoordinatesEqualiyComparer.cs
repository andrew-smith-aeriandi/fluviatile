using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public class NodePairCoordinatesEqualiyComparer : IEqualityComparer<NodePair>
    {
        public static readonly NodePairCoordinatesEqualiyComparer Default = new();
        private static readonly NodeCoordinatesEqualityComparer nodeComparer = NodeCoordinatesEqualityComparer.Default;

        public bool Equals(NodePair x, NodePair y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return nodeComparer.Equals(x.Node1, y.Node1) &&
                nodeComparer.Equals(x.Node2, y.Node2);
        }

        public int GetHashCode(NodePair obj)
        {
            unchecked
            {
                return obj.Node1.Coordinates.GetHashCode() * 7867
                     + obj.Node2.Coordinates.GetHashCode();
            }
        }
    }
}
