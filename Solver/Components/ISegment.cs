namespace Solver.Components;

public interface ISegment
{
    public int Count { get; }

    public int TileCount { get; }

    public int TerminationCount { get; }

    public int Rotation { get; }

    public ILinkable? First { get; }

    public ILinkable? Last { get; }

    public IEnumerable<ILinkable> Links { get; }

    public IEnumerable<Tile> Tiles { get; }

    public IEnumerable<Termination> Terminations { get; }
}
