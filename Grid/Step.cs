using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public class Step
    {
        public Step(Node node, Direction direction)
            : this(node, direction, Torsion.None, null)
        {
        }

        public Step(Node node, Direction direction, Torsion twist, Step previous)
        {
            Node = node;
            Previous = previous;
            Direction = direction;

            if (previous != null)
            {
                Count = previous.Count + 1;
                Torsion = previous.Torsion + twist;
            }
            else
            {
                Count = 0;
                Torsion = Torsion.None;
            }

            //BitMask = new byte[12];
            //if (previous?.BitMask != null)
            //{
            //    Array.Copy(previous.BitMask, BitMask, 12);
            //}

            //SetNodeAsIncluded(node);
        }

        public Node Node { get; }

        public int Count { get; }

        public Direction Direction { get; }

        public Torsion Torsion { get; }

        public Step Previous { get; }

        //public byte[] BitMask { get; }

        //private void SetNodeAsIncluded(Node node)
        //{
        //    var index = node.Index / 8;
        //    if (index < 12)
        //    {
        //        BitMask[index] |= (byte)(1 << (7 - node.Index % 8));
        //    }
        //}

        //public bool IsNodeIncluded(Node node)
        //{
        //    var index = node.Index / 8;
        //    if (index < 12)
        //    {
        //        return (BitMask[index] & (1 << (7 - node.Index % 8))) > 0;
        //    }

        //    return false;
        //}

        public IEnumerable<Node> AllNodes
        {
            get
            {
                yield return Node;

                for (var iterator = Previous; iterator != null; iterator = iterator.Previous)
                {
                    yield return iterator.Node;
                }
            }
        }

        public override string ToString()
        {
            return string.Join(" < ", AllNodes);
        }
    }
}
