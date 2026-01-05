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

    public Coordinates Coordinates { get; }

    public Edge Border { get; private set; }

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
