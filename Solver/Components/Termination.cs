using Solver.Framework;

namespace Solver.Components;

public class Termination : IComponent, ILinkable
{
    public Termination(
        Coordinates coordinates,
        Edge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);

        if (!edge.IsBorder)
        {
            throw new ArgumentException($"Edge must on border: {edge}", nameof(edge));
        }

        Coordinates = coordinates;
        Border = edge;
    }

    public Coordinates Coordinates { get; }

    public Edge Border { get; }

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
