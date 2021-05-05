using System.Collections.Generic;
using System.Linq;

namespace Combinations
{
    public class RouteParser<T>
    {
        private readonly IValueParser<T> _valueParser;
        private readonly Dictionary<T, INode<T>> _nodeLookup;

        public RouteParser(IValueParser<T> valueParser, Tableau<T> tableau)
        {
            _valueParser = valueParser;
            _nodeLookup = tableau.Nodes
                .Concat(tableau.TerminalNodes)
                .ToDictionary(n => n.Value);
        }

        public IRoute<INode<T>> Parse(string text)
        {
            var nodes = text.Split(';')
                .Select(s => _valueParser.Parse(s))
                .Select(c => _nodeLookup[c])
                .Reverse();

            return nodes.Aggregate(
                (IRoute<INode<T>>)null,
                (current, node) => new Route<INode<T>>(node, current));
        }
    }
}
