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
            Twist = twist;

            if (previous is not null)
            {
                Count = previous.Count + 1;
                Torsion = previous.Torsion + twist;
            }
            else
            {
                Count = 0;
                Torsion = Torsion.None;
            }
        }

        public Node Node { get; }

        public int Count { get; }

        public Direction Direction { get; }

        public Torsion Torsion { get; }

        public Torsion Twist { get; }

        public Step Previous { get; }

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
