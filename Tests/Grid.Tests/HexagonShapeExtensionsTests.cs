using Fluviatile.Grid;
using System.Linq;
using Xunit;

namespace Grid.Tests
{
    public class HexagonShapeExtensionsTests
    {
        [Fact]
        public void Centre_ReturnsExpectedCoordinates()
        {
            // Arrange
            var shape = new Hexagon(2);

            // Act
            var result = shape.Centre;

            // Assert
            Assert.Equal(new Coordinates(6, 6), result);
        }

        [Fact]
        public void SymmetryTransformations_ReturnsExpectedSymmetryTransformations()
        {
            // Arrange
            var shape = new Hexagon(2);
            var coordinates = new Coordinates(1, -1);

            // Act
            var results = shape.SymmetryTransformations
                .Select(symmetry => symmetry.Transform(coordinates))
                .ToList();

            // Assert
            Assert.Equal(
                [
                    new(1, -1),
                    new(8, 1),
                    new(13, 8),
                    new(11, 13),
                    new(4, 11),
                    new(-1, 4),
                    new(8, 13),
                    new(11, 4),
                    new(-1, 1),
                    new(1, 8),
                    new(4, -1),
                    new(13, 11)
                ],
                results);
        }

        [Fact]
        public void CreateTableau_ReturnsPopulatedTableau()
        {
            // Arrange
            var shape = new Hexagon(1);

            // Act
            var tableau = shape.CreateTableau();

            // Assert
            Assert.NotNull(tableau);
            Assert.Equal(shape, tableau.Shape);
            Assert.Equal("Tableau: Hexagon(1)", tableau.ToString());

            Assert.Equal(new Coordinates(1, -1), tableau.TerminalNodes[0].Coordinates);
            Assert.Equal(new Coordinates(2, 1), tableau.TerminalNodes[0].Links[Hexagon.Azimuth180].Coordinates);

            Assert.Equal(new Coordinates(5, 1), tableau.TerminalNodes[1].Coordinates);
            Assert.Equal(new Coordinates(4, 2), tableau.TerminalNodes[1].Links[Hexagon.Azimuth240].Coordinates);

            Assert.Equal(new Coordinates(7, 5), tableau.TerminalNodes[2].Coordinates);
            Assert.Equal(new Coordinates(5, 4), tableau.TerminalNodes[2].Links[Hexagon.Azimuth300].Coordinates);

            Assert.Equal(new Coordinates(5, 7), tableau.TerminalNodes[3].Coordinates);
            Assert.Equal(new Coordinates(4, 5), tableau.TerminalNodes[3].Links[Hexagon.Azimuth000].Coordinates);

            Assert.Equal(new Coordinates(1, 5), tableau.TerminalNodes[4].Coordinates);
            Assert.Equal(new Coordinates(2, 4), tableau.TerminalNodes[4].Links[Hexagon.Azimuth060].Coordinates);

            Assert.Equal(new Coordinates(-1, 1), tableau.TerminalNodes[5].Coordinates);
            Assert.Equal(new Coordinates(1, 2), tableau.TerminalNodes[5].Links[Hexagon.Azimuth120].Coordinates);

            Assert.Equal(new Coordinates(2, 1), tableau.Nodes[0].Coordinates);
            Assert.Equal(new Coordinates(4, 2), tableau.Nodes[0].Links[Hexagon.Azimuth120].Coordinates);
            Assert.Equal(new Coordinates(1, 2), tableau.Nodes[0].Links[Hexagon.Azimuth240].Coordinates);
            Assert.Equal(new Coordinates(1, -1), tableau.Nodes[0].Links[Hexagon.Azimuth000].Coordinates);

            Assert.Equal(new Coordinates(4, 2), tableau.Nodes[1].Coordinates);
            Assert.Equal(new Coordinates(5, 4), tableau.Nodes[1].Links[Hexagon.Azimuth180].Coordinates);
            Assert.Equal(new Coordinates(2, 1), tableau.Nodes[1].Links[Hexagon.Azimuth300].Coordinates);
            Assert.Equal(new Coordinates(5, 1), tableau.Nodes[1].Links[Hexagon.Azimuth060].Coordinates);

            Assert.Equal(new Coordinates(5, 4), tableau.Nodes[2].Coordinates);
            Assert.Equal(new Coordinates(7, 5), tableau.Nodes[2].Links[Hexagon.Azimuth120].Coordinates);
            Assert.Equal(new Coordinates(4, 5), tableau.Nodes[2].Links[Hexagon.Azimuth240].Coordinates);
            Assert.Equal(new Coordinates(4, 2), tableau.Nodes[2].Links[Hexagon.Azimuth000].Coordinates);

            Assert.Equal(new Coordinates(4, 5), tableau.Nodes[3].Coordinates);
            Assert.Equal(new Coordinates(5, 7), tableau.Nodes[3].Links[Hexagon.Azimuth180].Coordinates);
            Assert.Equal(new Coordinates(2, 4), tableau.Nodes[3].Links[Hexagon.Azimuth300].Coordinates);
            Assert.Equal(new Coordinates(5, 4), tableau.Nodes[3].Links[Hexagon.Azimuth060].Coordinates);

            Assert.Equal(new Coordinates(2, 4), tableau.Nodes[4].Coordinates);
            Assert.Equal(new Coordinates(4, 5), tableau.Nodes[4].Links[Hexagon.Azimuth120].Coordinates);
            Assert.Equal(new Coordinates(1, 5), tableau.Nodes[4].Links[Hexagon.Azimuth240].Coordinates);
            Assert.Equal(new Coordinates(1, 2), tableau.Nodes[4].Links[Hexagon.Azimuth000].Coordinates);

            Assert.Equal(new Coordinates(1, 2), tableau.Nodes[5].Coordinates);
            Assert.Equal(new Coordinates(2, 4), tableau.Nodes[5].Links[Hexagon.Azimuth180].Coordinates);
            Assert.Equal(new Coordinates(-1, 1), tableau.Nodes[5].Links[Hexagon.Azimuth300].Coordinates);
            Assert.Equal(new Coordinates(2, 1), tableau.Nodes[5].Links[Hexagon.Azimuth060].Coordinates);
        }
    }
}
