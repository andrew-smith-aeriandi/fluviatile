using Solver.Components;

namespace Solver.Framework;

public static class ComponentTypes
{
    public readonly static ComponentType Tableau = ComponentType.Create<Tableau>();
    public readonly static ComponentType Aisle = ComponentType.Create<Aisle>();
    public readonly static ComponentType Tile = ComponentType.Create<Tile>();
    public readonly static ComponentType Edge = ComponentType.Create<Edge>();
    public readonly static ComponentType Thalweg = ComponentType.Create<Thalweg>();
    public readonly static ComponentType ThalwegSegment = ComponentType.Create<Thalweg.Segment>();
    public readonly static ComponentType Termination = ComponentType.Create<Termination>();
}
