using Solver.Framework;

namespace Solver.Components;

public class Termination : IComponent, ILinkable, IFreezable
{
    private bool _frozen = false;

    public Termination(Coordinates coordinates)
    {
        Coordinates = coordinates;
    }

    public Termination(
        Coordinates coordinates,
        Edge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);

        if (!edge.IsBorder)
        {
            throw new ArgumentException("Edge must be on the border of the grid", nameof(edge));
        }

        Coordinates = coordinates;
        Border = edge;
        _frozen = true;
    }

    public bool IsFrozen => _frozen;

    public void Freeze()
    {
        _frozen = true;
    }

    /// <summary>
    /// Coordinates of the centre of the "virtual" tile that is adjacent to a border edge where the channel exits the grid
    /// </summary>
    public Coordinates Coordinates { get; }

    /// <summary>
    /// References the border edge where the channel exits the grid
    /// </summary>
    public Edge Border { get; private set; }

    /// <summary>
    /// Always returns true 
    /// </summary>
    public bool IsTerminal => true;

    public override int GetHashCode()
    {
        return Coordinates.GetHashCode();
    }

    public override string ToString()
    {
        return $"Exit:{Coordinates}";
    }
}
