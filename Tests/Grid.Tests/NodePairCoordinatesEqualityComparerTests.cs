using Fluviatile.Grid;
using Xunit;

namespace Grid.Tests
{
    public class NodePairCoordinatesEqualityComparerTests
    {
        [Fact]
        public void Equals_WhenBothNull_ReturnsTrue()
        {
            // Arrange
            var comparer = NodePairCoordinatesEqualiyComparer.Default;

            // Act
            var result = comparer.Equals(null, null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equals_WhenOneNull_ReturnsFalse()
        {
            // Arrange
            var nodePair = new NodePair(
                new Node(12, new Coordinates(10, 5)),
                new Node(13, new Coordinates(7, 2)));

            var comparer = NodePairCoordinatesEqualiyComparer.Default;

            // Act
            var result1 = comparer.Equals(nodePair, null);
            var result2 = comparer.Equals(null, nodePair);

            // Assert
            Assert.False(result1);
            Assert.False(result2);
        }

        [Fact]
        public void Equals_WhenBothHaveSameCoordinates_ReturnsTrue()
        {
            // Arrange
            var nodePair1 = new NodePair(
                new Node(12, new Coordinates(10, 5)),
                new Node(13, new Coordinates(7, 2)));

            var nodePair2 = new NodePair(
                new Node(13, new Coordinates(7, 2)),
                new Node(12, new Coordinates(10, 5)));

            var comparer = NodePairCoordinatesEqualiyComparer.Default;

            // Act
            var result = comparer.Equals(nodePair1, nodePair2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equals_WhenCoordinatesDifferInNode1_ReturnsFalse()
        {
            // Arrange
            var nodePair1 = new NodePair(
                new Node(12, new Coordinates(10, 5)),
                new Node(13, new Coordinates(7, 2)));

            var nodePair2 = new NodePair(
                new Node(12, new Coordinates(1, 5)),
                new Node(13, new Coordinates(7, 2)));

            var comparer = NodePairCoordinatesEqualiyComparer.Default;

            // Act
            var result = comparer.Equals(nodePair1, nodePair2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_WhenCoordinatesDifferInNode2_ReturnsFalse()
        {
            // Arrange
            var nodePair1 = new NodePair(
                new Node(12, new Coordinates(10, 5)),
                new Node(13, new Coordinates(7, 2)));

            var nodePair2 = new NodePair(
                new Node(12, new Coordinates(10, 5)),
                new Node(13, new Coordinates(2, 7)));

            var comparer = NodePairCoordinatesEqualiyComparer.Default;

            // Act
            var result = comparer.Equals(nodePair1, nodePair2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetHashCode_WhenBothHaveSameCoordinates_ReturnsSameValue()
        {
            // Arrange
            var nodePair1 = new NodePair(
                new Node(12, new Coordinates(10, 5)),
                new Node(13, new Coordinates(7, 2)));

            var nodePair2 = new NodePair(
                new Node(13, new Coordinates(7, 2)),
                new Node(12, new Coordinates(10, 5)));

            var comparer = NodePairCoordinatesEqualiyComparer.Default;

            // Act
            var hashCode1 = comparer.GetHashCode(nodePair1);
            var hashCode2 = comparer.GetHashCode(nodePair2);

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        [Fact]
        public void GetHashCode_WhenCoordinatesDifferInNode1_ReturnsDifferentValue()
        {
            // Arrange
            var nodePair1 = new NodePair(
                new Node(12, new Coordinates(10, 5)),
                new Node(13, new Coordinates(7, 2)));

            var nodePair2 = new NodePair(
                new Node(12, new Coordinates(1, 5)),
                new Node(13, new Coordinates(7, 2)));

            var comparer = NodePairCoordinatesEqualiyComparer.Default;

            // Act
            var hashCode1 = comparer.GetHashCode(nodePair1);
            var hashCode2 = comparer.GetHashCode(nodePair2);

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }

        [Fact]
        public void GetHashCode_WhenCoordinatesDifferInNode2_ReturnsDifferentValue()
        {
            // Arrange
            var nodePair1 = new NodePair(
                new Node(12, new Coordinates(10, 5)),
                new Node(13, new Coordinates(7, 2)));

            var nodePair2 = new NodePair(
                new Node(12, new Coordinates(10, 5)),
                new Node(13, new Coordinates(7, 3)));

            var comparer = NodePairCoordinatesEqualiyComparer.Default;

            // Act
            var hashCode1 = comparer.GetHashCode(nodePair1);
            var hashCode2 = comparer.GetHashCode(nodePair2);

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }
    }
}
