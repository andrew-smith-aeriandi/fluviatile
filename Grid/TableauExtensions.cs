using System.Collections.Generic;
using System.Linq;

namespace Fluviatile.Grid
{
    public static class TableauExtensions
    {
        public static IEnumerable<NodePair> TerminalNodeUniqueCombinations(this Tableau tableau)
        {
            var symmetryTransformations = tableau.Shape.SymmetryTransformations();
            var nodePairHashSet = new HashSet<NodePair>(NodePairCoordinatesEqualiyComparer.Default);
            var centre = tableau.Shape.Centre;

            for (var i = 0; i < tableau.TerminalNodes.Count - 1; i++)
            {
                for (var j = i + 1; j < tableau.TerminalNodes.Count; j++)
                {
                    var candidateNodePair = new NodePair(tableau.TerminalNodes[i], tableau.TerminalNodes[j]);
                    var transformedNodePairs = symmetryTransformations
                        .Select(transformation => new NodePair(
                            tableau[transformation(candidateNodePair.Node1.Coordinates - centre) + centre],
                            tableau[transformation(candidateNodePair.Node2.Coordinates - centre) + centre]))
                        .ToList();

                    var isIntersection = transformedNodePairs.Any(nodePair => nodePairHashSet.Contains(nodePair));
                    if (!isIntersection)
                    {
                        nodePairHashSet.Add(candidateNodePair);
                    }
                }
            }

            return nodePairHashSet;
        }

    }
}
