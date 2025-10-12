namespace Solver.Components;

public static class ThalwegExtensions
{
    public static Thalweg Clone(this Thalweg thalweg)
    {
        ArgumentNullException.ThrowIfNull(thalweg);

        return new Thalweg(
            thalweg.Grid,
            thalweg.TileCount,
            thalweg.Segments.Select(segment => segment.Links));
    }
}
