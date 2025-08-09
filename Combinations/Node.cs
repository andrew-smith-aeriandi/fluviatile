using System.Collections.Generic;

namespace Combinations;

public class Node : INode<Coordinate>
{
    private readonly HashSet<INode<Coordinate>> _links;
    private readonly List<int> _countIndexes;

    public Node(Coordinate coordinate)
    {
        Value = coordinate;
        _links = new HashSet<INode<Coordinate>>();
        _countIndexes = new List<int>();
    }

    public Coordinate Value { get; }

    public IEnumerable<INode<Coordinate>> Links => _links;

    public void AddLink(INode<Coordinate> link)
    {
        _links.Add(link);
    }

    public IEnumerable<int> CountIndexes => _countIndexes;

    public void AddCountIndex(int countIndex)
    {
        _countIndexes.Add(countIndex);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

