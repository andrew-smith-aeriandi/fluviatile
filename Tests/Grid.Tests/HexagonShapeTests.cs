using Fluviatile.Grid;
using System;
using Xunit;

namespace Grid.Tests
{
    public class HexagonShapeTests
    {
        [Fact]
        public void Constructor_PopulatesProperties()
        {
            // Act
            var shape = new Hexagon(2);

            // Assert
            Assert.Equal("Hexagon", shape.Name);
            Assert.Equal(2, shape.Size);
        }

        [Fact]
        public void Constructor_WithInvalidSize_Throws()
        {
            // Act
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Hexagon(0));

            // Assert
            Assert.Equal("size", ex.ParamName);
        }

        [Fact]
        public void ToString_ReturnsExpectedString()
        {
            // Arrange
            var shape = new Hexagon(2);

            // Act
            var result = shape.ToString();

            // Assert
            Assert.Equal("Hexagon(2)", result);
        }

        [Fact]
        public void IsFullTurn_WhenFullTurn_ReturnsTrue()
        {
            // Arrange
            var hexagon = new Hexagon(2);

            var step0 = new Step(
                new Node(5, new Coordinates(8, 13)),
                new Direction(2));

            var step1 = new Step(
                new Node(6, new Coordinates(7, 11)),
                new Direction(1),
                new Torsion(-1),
                step0);

            var step2 = new Step(
                new Node(7, new Coordinates(5, 10)),
                new Direction(2),
                new Torsion(1),
                step1);

            var step3 = new Step(
                new Node(8, new Coordinates(4, 8)),
                new Direction(3),
                new Torsion(1),
                step2);

            var step4 = new Step(
                new Node(9, new Coordinates(5, 7)),
                new Direction(2),
                new Torsion(-1),
                step3);

            var step5 = new Step(
                new Node(10, new Coordinates(4, 5)),
                new Direction(3),
                new Torsion(1),
                step4);

            var step6 = new Step(
                new Node(11, new Coordinates(5, 4)),
                new Direction(4),
                new Torsion(1),
                step5);

            var step7 = new Step(
                new Node(12, new Coordinates(7, 5)),
                new Direction(5),
                new Torsion(1),
                step6);

            var step8 = new Step(
                new Node(13, new Coordinates(8, 7)),
                new Direction(4),
                new Torsion(-1),
                step7);

            var step9 = new Step(
                new Node(14, new Coordinates(10, 8)),
                new Direction(5),
                new Torsion(1),
                step8);

            var step10 = new Step(
                new Node(15, new Coordinates(11, 10)),
                new Direction(0),
                new Torsion(1),
                step9);

            var step11 = new Step(
                new Node(16, new Coordinates(10, 11)),
                new Direction(1),
                new Torsion(1),
                step10);

            // Act + Assert
            Assert.False(hexagon.IsFullTurn(step1, step1));
            Assert.False(hexagon.IsFullTurn(step1, step2));
            Assert.False(hexagon.IsFullTurn(step1, step3));
            Assert.False(hexagon.IsFullTurn(step1, step4));
            Assert.False(hexagon.IsFullTurn(step1, step5));
            Assert.False(hexagon.IsFullTurn(step1, step6));
            Assert.False(hexagon.IsFullTurn(step1, step7));
            Assert.False(hexagon.IsFullTurn(step1, step8));
            Assert.False(hexagon.IsFullTurn(step1, step9));
            Assert.False(hexagon.IsFullTurn(step1, step10));
            Assert.True(hexagon.IsFullTurn(step1, step11));
        }

    }
}
