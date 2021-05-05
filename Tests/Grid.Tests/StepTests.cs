using Fluviatile.Grid;
using Xunit;

namespace Grid.Tests
{
    public class StepTests
    {
        [Fact]
        public void Constructor_WithTerminalNode_PopulatesProperties()
        {
            // Act
            var step = new Step(
                new Node(5, new Coordinates(8, 13)),
                new Direction(2));

            // Assert
            Assert.Equal(new Coordinates(8, 13), step.Node.Coordinates);
            Assert.Equal(2, step.Direction.Value);
            Assert.Equal(0, step.Torsion.Value);
            Assert.Equal(0, step.Count);
            Assert.Null(step.Previous);
            Assert.Equal("(8,13)", step.ToString());
        }

        [Fact]
        public void Constructor_WithPreviousNode_PopulatesProperties()
        {
            // Act
            var step0 = new Step(
                new Node(5, new Coordinates(8, 13)),
                new Direction(2));

            var step = new Step(
                new Node(6, new Coordinates(7, 11)),
                new Direction(3),
                new Torsion(1),
                step0);

            // Assert
            Assert.Equal(new Coordinates(7, 11), step.Node.Coordinates);
            Assert.Equal(3, step.Direction.Value);
            Assert.Equal(1, step.Torsion.Value);
            Assert.Equal(1, step.Count);
            Assert.Equal(new Coordinates(8, 13), step.Previous.Node.Coordinates);
            Assert.Equal("(7,11) < (8,13)", step.ToString());
        }

        [Fact]
        public void AllNodes_WithTerminalNode_EnumeratesNodes()
        {
            // Act
            var step = new Step(
                new Node(5, new Coordinates(8, 13)),
                new Direction(2));

            // Assert
            Assert.Collection(
                step.AllNodes,
                node => Assert.Equal(new Coordinates(8, 13), node.Coordinates));
        }

        [Fact]
        public void AllNodes_WithPreviousNode_EnumeratesNodes()
        {
            // Act
            var step0 = new Step(
                new Node(5, new Coordinates(8, 13)),
                new Direction(2));

            var step = new Step(
                new Node(6, new Coordinates(7, 11)),
                new Direction(3),
                new Torsion(1),
                step0);

            // Assert
            Assert.Collection(
                step.AllNodes,
                node => Assert.Equal(new Coordinates(7, 11), node.Coordinates),
                node => Assert.Equal(new Coordinates(8, 13), node.Coordinates));
        }
    }
}
