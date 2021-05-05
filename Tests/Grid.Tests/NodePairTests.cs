using Fluviatile.Grid;
using System;
using Xunit;

namespace Grid.Tests
{
    public class NodePairTests
    {
        [Fact]
        public void Constructor_WithNullFirstArgument()
        {
            // Act + Assert
            var ex = Assert.Throws<ArgumentNullException>(
                () => new NodePair(null, new Node(2, new Coordinates(1, 8))));

            Assert.Equal("node1", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullSecondArgument()
        {
            // Act + Assert
            var ex = Assert.Throws<ArgumentNullException>(
                () => new NodePair(new Node(1, new Coordinates(-1, 4)), null));

            Assert.Equal("node2", ex.ParamName);
        }

        [Fact]
        public void ToString_ReturnsExpectedString()
        {
            // Arrange
            var node1 = new Node(2, new Coordinates(-1, 4));
            var node2 = new Node(1, new Coordinates(1, 8));
            var nodePair = new NodePair(node1, node2);

            // Act
            var result = nodePair.ToString();

            // Assert
            Assert.Equal("[(1,8), (-1,4)]", result);
        }

        [Fact]
        public void Constructor_WithNodesSuppliedInIndexOrder_PopulatesNodesAsSupplied()
        {
            // Arrange
            var node1 = new Node(1, new Coordinates(-1, 4));
            var node2 = new Node(2, new Coordinates(1, 8));

            // Act
            var nodePair = new NodePair(node1, node2);

            // Assert
            Assert.Equal(node1, nodePair.Node1);
            Assert.Equal(node2, nodePair.Node2);
        }

        [Fact]
        public void Constructor_WithNodesSuppliedInReverseIndexOrder_PopulatesNodesReversed()
        {
            // Arrange
            var node1 = new Node(2, new Coordinates(-1, 4));
            var node2 = new Node(1, new Coordinates(1, 8));

            // Act
            var nodePair = new NodePair(node1, node2);

            // Assert
            Assert.Equal(node2, nodePair.Node1);
            Assert.Equal(node1, nodePair.Node2);
        }
    }
}
