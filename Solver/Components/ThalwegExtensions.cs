using System.Diagnostics;

namespace Solver.Components;

public static class ThalwegExtensions
{
    public static void CopyTerminations(this Thalweg source, Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(tableau);

        foreach (var sourceTermination in source.Terminations)
        {
            if (!tableau.Edges.TryGetValue(sourceTermination.Border.GetDefaultKey(), out var edge))
            {
                throw new UnreachableException();
            }

            var termination = new Termination(sourceTermination.Coordinates, edge);

            if (!tableau.Thalweg.TryAddTermination(termination))
            {
                throw new UnreachableException();
            }
        }
    }

    public static void CopySegments(this Thalweg source, Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(tableau);

        foreach (var sourceSegment in source.Segments)
        {
            var links = sourceSegment.Links.Select(link => link switch
            {
                Tile sourceTile => tableau.TryGetTile(
                    sourceTile.Coordinates,
                    out var tile)
                        ? (ILinkable)tile!
                        : throw new UnreachableException(),
                Termination sourceTermination => tableau.Thalweg.TryGetTermination(
                    sourceTermination.Coordinates,
                    out var termination)
                        ? (ILinkable)termination!
                        : throw new UnreachableException(),
                _ => throw new UnreachableException()
            });

            tableau.Thalweg.CreateSegment(links);
        }
    }
}
