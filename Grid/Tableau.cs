using System.Collections.Generic;
using System.Linq;

namespace Fluviatile.Grid
{
    public class Tableau
    {
        private readonly Node[] _nodes;
        private readonly TerminalNode[] _terminalNodes;
        private readonly Dictionary<Coordinates, Node> _coordinatesLookup;

        public Tableau(
            Shape shape,
            IEnumerable<Node> nodes,
            IEnumerable<TerminalNode> terminalNodes)
        {
            Shape = shape;
            _nodes = [.. nodes];
            _terminalNodes = [.. terminalNodes];

            _coordinatesLookup = _nodes
                .Concat(_terminalNodes)
                .ToDictionary(node => node.Coordinates, node => node);
        }

        public Shape Shape { get; }

        public IReadOnlyList<Node> Nodes => _nodes;

        public IReadOnlyList<TerminalNode> TerminalNodes => _terminalNodes;

        public Node this[Coordinates index] => _coordinatesLookup[index];

        public override string ToString()
        {
            return $"Tableau: {Shape}";
        }
    }
}
