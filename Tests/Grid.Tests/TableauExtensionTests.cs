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
                new(0, new Coordinates(2, 1)),
                new(1, new Coordinates(4, 2)),
                new(2, new Coordinates(5, 4)),
                new(3, new Coordinates(4, 5)),
                new(4, new Coordinates(2, 4)),
                new(5, new Coordinates(1, 2)),
            };

            var aisles = new List<Aisle>
            {
                new(0, 0),
                new(1, 0),
                new(2, 0),
                new(0, 1),
                new(1, 1),
                new(2, 1)
            };

            var terminalNodes = new List<TerminalNode>
            {
                new(0, new Coordinates(1, -1), [(Hexagon.Azimuth180, nodes[4])], aisles[0]),
                new(1, new Coordinates(5, 1), [(Hexagon.Azimuth240, nodes[5])], aisles[1]),
                new(2, new Coordinates(7, 5), [(Hexagon.Azimuth300, nodes[0])], aisles[2]),
                new(3, new Coordinates(5, 7), [(Hexagon.Azimuth000, nodes[1])], aisles[3]),
                new(4, new Coordinates(1, 5), [(Hexagon.Azimuth060, nodes[2])], aisles[4]),
                new(5, new Coordinates(-1, 1), [(Hexagon.Azimuth120, nodes[3])], aisles[5])
            };

            var tableau = new Tableau(shape, nodes, terminalNodes);

            // Act
            var combinations = tableau.TerminalNodeUniqueCombinations().ToList();

            // Assert
            Assert.Equal(
                [
                    new(
                        new Node(0, new Coordinates(1, -1)),
                        new Node(0, new Coordinates(5, 1))),

                    new(
                        new Node(0, new Coordinates(1, -1)),
                        new Node(0, new Coordinates(7, 5))),

                    new(
                        new Node(0, new Coordinates(1, -1)),
                        new Node(0, new Coordinates(5, 7)))
                ],
                combinations,
                NodePairCoordinatesEqualiyComparer.Default);
        }
    }
}
