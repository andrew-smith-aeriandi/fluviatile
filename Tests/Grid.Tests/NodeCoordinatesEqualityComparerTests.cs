using Fluviatile.Grid;
using Xunit;

namespace Grid.Tests
{
    public class NodeCoordinatesEqualityComparerTests
    {
        [Fact]
        public void Equals_WhenBothNull_ReturnsTrue()
        {
            // Arrange
            var comparer = NodeCoordinatesEqualityComparer.Default;

            // Act
            var result = comparer.Equals(null, null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equals_WhenOneNull_ReturnsFalse()
        {
            // Arrange
            var node = new Node(12, new Coordinates(10, 5));
            var comparer = NodeCoordinatesEqualityComparer.Default;

            // Act
            var result1 = comparer.Equals(node, null);
            var result2 = comparer.Equals(null, node);

            // Assert
            Assert.False(result1);
            Assert.False(result2);
        }

        [Fact]
        public void Equals_WhenBothHaveSameCoordiantes_ReturnsTrue()
        {
            // Arrange
            var node1 = new Node(12, new Coordinates(10, 5));
            var node2 = new Node(13, new Coordinates(10, 5));
            var comparer = NodeCoordinatesEqualityComparer.Default;

            // Act
            var result = comparer.Equals(node1, node2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetHashCode_WhenBothHaveSameCoordiantes_ReturnsSameValue()
        {
            // Arrange
            var node1 = new Node(12, new Coordinates(10, 5));
            var node2 = new Node(13, new Coordinates(10, 5));
            var comparer = NodeCoordinatesEqualityComparer.Default;

            // Act
            var hashCode1 = comparer.GetHashCode(node1);
            var hashCode2 = comparer.GetHashCode(node2);

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        [Fact]
        public void GetHashCode_WhenCoordiantesDiffer_ReturnsDifferentValue()
        {
            // Arrange
            var node1 = new Node(12, new Coordinates(10, 5));
            var node2 = new Node(12, new Coordinates(10, 8));
            var comparer = NodeCoordinatesEqualityComparer.Default;

            // Act
            var hashCode1 = comparer.GetHashCode(node1);
            var hashCode2 = comparer.GetHashCode(node2);

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }
    }
}
