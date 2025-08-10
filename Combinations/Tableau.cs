using System.Collections.Generic;

namespace Combinations;

public class Tableau<T>
{
    public Tableau(IEnumerable<INode<T>> nodes, IEnumerable<INode<T>> terminalNodes)
    {
        Nodes = [.. nodes];
        TerminalNodes = [.. terminalNodes];
    }

    public HashSet<INode<T>> Nodes { get; }

    public HashSet<INode<T>> TerminalNodes { get; }
}
