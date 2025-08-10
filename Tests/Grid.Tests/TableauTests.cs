using Fluviatile.Grid;
using System.Collections.Generic;
using Xunit;

namespace Grid.Tests
{
    public class TableauTests
    {
        [Fact]
        public void Constructor_PopulatesProperties()
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
                new(5, new Coordinates(1, 2))
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
                new(0, new Coordinates(1, -1), [(Hexagon.Azimuth180, nodes[0])], aisles[0]),
                new(1, new Coordinates(5, 1), [(Hexagon.Azimuth240, nodes[1])], aisles[1]),
                new(2, new Coordinates(7, 5), [(Hexagon.Azimuth300, nodes[2])], aisles[2]),
                new(3, new Coordinates(5, 7), [(Hexagon.Azimuth000, nodes[3])], aisles[3]),
                new(4, new Coordinates(1, 5), [(Hexagon.Azimuth060, nodes[4])], aisles[4]),
                new(5, new Coordinates(-1, 1), [(Hexagon.Azimuth120, nodes[5])], aisles[5])
            };

            // Act
            var tableau = new Tableau(shape, nodes, terminalNodes);

            // Assert
            Assert.NotNull(tableau);
            Assert.Equal("Hexagon", tableau.Shape.Name);
            Assert.Equal(6, tableau.Shape.Edges);
            Assert.Equal(1, tableau.Shape.Size);
            Assert.Equal(6, tableau.Nodes.Count);
            Assert.Equal(6, tableau.TerminalNodes.Count);
            Assert.Equal("Tableau: Hexagon(1)", tableau.ToString());
        }

        [Fact]
        public void Indexer_WithValidCoordinates_ReturnsNode()
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
                new(5, new Coordinates(1, 2))
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
                new(0, new Coordinates(1, -1), [(Hexagon.Azimuth180, nodes[0])], aisles[0]),
                new(1, new Coordinates(5, 1), [(Hexagon.Azimuth240, nodes[1])], aisles[1]),
                new(2, new Coordinates(7, 5), [(Hexagon.Azimuth300, nodes[2])], aisles[2]),
                new(3, new Coordinates(5, 7), [(Hexagon.Azimuth000, nodes[3])], aisles[3]),
                new(4, new Coordinates(1, 5), [(Hexagon.Azimuth060, nodes[4])], aisles[4]),
                new(5, new Coordinates(-1, 1), [(Hexagon.Azimuth120, nodes[5])], aisles[5])
            };

            // Act
            var tableau = new Tableau(shape, nodes, terminalNodes);

            // Assert
            Assert.Same(nodes[0], tableau[new Coordinates(2, 1)]);
            Assert.Same(nodes[1], tableau[new Coordinates(4, 2)]);
            Assert.Same(nodes[2], tableau[new Coordinates(5, 4)]);
            Assert.Same(nodes[3], tableau[new Coordinates(4, 5)]);
            Assert.Same(nodes[4], tableau[new Coordinates(2, 4)]);
            Assert.Same(nodes[5], tableau[new Coordinates(1, 2)]);

            Assert.Same(terminalNodes[0], tableau[new Coordinates(1, -1)]);
            Assert.Same(terminalNodes[1], tableau[new Coordinates(5, 1)]);
            Assert.Same(terminalNodes[2], tableau[new Coordinates(7, 5)]);
            Assert.Same(terminalNodes[3], tableau[new Coordinates(5, 7)]);
            Assert.Same(terminalNodes[4], tableau[new Coordinates(1, 5)]);
            Assert.Same(terminalNodes[5], tableau[new Coordinates(-1, 1)]);
        }
    }
}
