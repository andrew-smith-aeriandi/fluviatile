using Fluviatile.Grid;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Grid.Tests
{
    public class TableauExtensionTests
    {
        [Fact]
        public void TerminalNodeUniqueCombinations()
        {
            // Arrange
            var shape = new Hexagon(1);

            var nodes = new List<Node>
            {
                new Node(0, new Coordinates(2, 1)),
                new Node(1, new Coordinates(4, 2)),
                new Node(2, new Coordinates(5, 4)),
                new Node(3, new Coordinates(4, 5)),
                new Node(4, new Coordinates(2, 4)),
                new Node(5, new Coordinates(1, 2)),
            };

            var aisles = new List<Aisle>
            {
                new Aisle(0, 0),
                new Aisle(1, 0),
                new Aisle(2, 0),
                new Aisle(0, 1),
                new Aisle(1, 1),
                new Aisle(2, 1)
            };

            var terminalNodes = new List<TerminalNode>
            {
                new TerminalNode(0, new Coordinates(1, -1), new[] { (Hexagon.Azimuth180, nodes[4]) }, aisles[0]),
                new TerminalNode(1, new Coordinates(5, 1), new[] { (Hexagon.Azimuth240, nodes[5]) }, aisles[1]),
                new TerminalNode(2, new Coordinates(7, 5), new[] { (Hexagon.Azimuth300, nodes[0]) }, aisles[2]),
                new TerminalNode(3, new Coordinates(5, 7), new[] { (Hexagon.Azimuth000, nodes[1]) }, aisles[3]),
                new TerminalNode(4, new Coordinates(1, 5), new[] { (Hexagon.Azimuth060, nodes[2]) }, aisles[4]),
                new TerminalNode(5, new Coordinates(-1, 1), new[] { (Hexagon.Azimuth120, nodes[3]) }, aisles[5])
            };

            var tableau = new Tableau(shape, nodes, terminalNodes);

            // Act
            var combinations = tableau.TerminalNodeUniqueCombinations().ToList();

            // Assert
            Assert.Equal(
                new List<NodePair>
                {
                    new NodePair(
                        new Node(0, new Coordinates(1, -1)),
                        new Node(0, new Coordinates(5, 1))),

                    new NodePair(
                        new Node(0, new Coordinates(1, -1)),
                        new Node(0, new Coordinates(7, 5))),

                    new NodePair(
                        new Node(0, new Coordinates(1, -1)),
                        new Node(0, new Coordinates(5, 7)))
                },
                combinations,
                NodePairCoordinatesEqualiyComparer.Default);
        }
    }
}
