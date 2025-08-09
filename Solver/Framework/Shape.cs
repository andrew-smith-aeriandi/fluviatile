namespace Solver.Framework;

public record Shape
{
    public required Orientation Orientation { get; init; }

    public required Coordinates[] VertexOffsets { get; init; }
}
