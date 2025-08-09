using System.Collections.Generic;

namespace Combinations;

public class Tableau<T>
{
    public Tableau(IEnumerable<INode<T>> nodes, IEnumerable<INode<T>> terminalNodes)
    {
        Nodes = new HashSet<INode<T>>(nodes);
        TerminalNodes = new HashSet<INode<T>>(terminalNodes);
    }

    public HashSet<INode<T>> Nodes { get; }

    public HashSet<INode<T>> TerminalNodes { get; }
}
