using System;

namespace Fluviatile.Grid
{
    public class NodePair
    {
        public NodePair(Node node1, Node node2)
        {
            ArgumentNullException.ThrowIfNull(node1);
            ArgumentNullException.ThrowIfNull(node2);

            if (node1.Index <= node2.Index)
            {
                Node1 = node1;
                Node2 = node2;
            }
            else
            {
                Node1 = node2;
                Node2 = node1;
            }
        }

        public Node Node1 { get; }

        public Node Node2 { get; }

        public override string ToString()
        {
            return $"[{Node1}, {Node2}]";
        }
    }
}
