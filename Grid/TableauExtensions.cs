using System.Collections.Generic;
using System.Linq;

namespace Fluviatile.Grid
{
    public static class TableauExtensions
    {
        public static Dictionary<SymmetryType, int> GetSymmetries(this Tableau tableau, NodePair nodePair)
        {
            return tableau.Shape.SymmetryTransformations
                .Where(symmetry =>
                    nodePair.Node1.Coordinates == symmetry.Transform(nodePair.Node2.Coordinates) &&
                    nodePair.Node2.Coordinates == symmetry.Transform(nodePair.Node1.Coordinates))
                .GroupBy(symmetry => symmetry.SymmetryType)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count());
        }

        public static IEnumerable<NodePair> TerminalNodeCombinations(this Tableau tableau)
        {
            for (var i = 0; i < tableau.TerminalNodes.Count - 1; i++)
            {
                for (var j = i + 1; j < tableau.TerminalNodes.Count; j++)
                {
                    yield return new NodePair(
                        tableau.TerminalNodes[i],
                        tableau.TerminalNodes[j]);
                }
            }
        }

        /// <summary>
        /// Considering the symmetry of the tableau, determine pairs of terminal nodes that are 
        /// distinct under the symmetry transformations.
        /// </summary>
        public static IEnumerable<NodePair> TerminalNodeUniqueCombinations(this Tableau tableau)
        {
            var symmetryTransformations = tableau.Shape.SymmetryTransformations;
            var nodePairHashSet = new HashSet<NodePair>(NodePairCoordinatesEqualiyComparer.Default);

            foreach (var nodePair in tableau.TerminalNodeCombinations())
            {
                var isEquivalentToExistingNodePair = symmetryTransformations
                    .Any(symmetry => nodePairHashSet.Contains(
                        new NodePair(
                            tableau[symmetry.Transform(nodePair.Node1.Coordinates)],
                            tableau[symmetry.Transform(nodePair.Node2.Coordinates)])));

                if (!isEquivalentToExistingNodePair && nodePairHashSet.Add(nodePair))
                {
                    yield return nodePair;
                }
            }
        }
    }
}
