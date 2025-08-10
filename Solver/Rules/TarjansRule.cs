using Solver.Components;
using Solver.Framework;

namespace Solver.Rules;

public class TarjansRule : IRule
{
    public override string ToString()
    {
        return nameof(TarjansRule);
    }

    public IEnumerable<Type> GetPertinentComponents()
    {
        yield return typeof(Tableau);
    }

    public void Invoke(IComponent component, INotifier notifier)
    {
        switch (component)
        {
            case Tableau tableau:
                InvokeInternal(tableau, notifier);
                break;
        }
    }

    private static void InvokeInternal(Tableau tableau, INotifier notifier)
    {
        var articulationPointTiles = Tarjan.GetArticulationPoints(
            tableau.Tiles.Values.Where(t => t.Resolution != Resolution.Empty),
            TileExtensions.GetPotentiallyLinkableTiles);

        var potentialChannels = articulationPointTiles
            .Where(t => !t.IsResolved)
            .ToList();

        if (potentialChannels.Count > 0)
        {
            foreach (var tile in potentialChannels)
            {
                //Console.WriteLine(tile.ToString());
            }
        }

    }
}
