using Fluviatile.Grid;
using Xunit;

namespace Grid.Tests
{
    public class CoordinatesTests
    {
        [Fact]
        public void Constructor_PopulatesValues()
        {
            // Act
            var coordinates = new Coordinates(4, 8);

            // Assert
            Assert.Equal(4, coordinates.X);
            Assert.Equal(8, coordinates.Y);
        }

        [Fact]
        public void ToString_ReturnsExpectedString()
        {
            // Arrange
            var coordinates = new Coordinates(4, 8);

            // Act
            var result = coordinates.ToString();

            // Assert
            Assert.Equal("(4,8)", result);
        }

        [Fact]
        public void AddOperator_Adds()
        {
            // Arrange
            var coordinates1 = new Coordinates(4, 8);
            var coordinates2 = new Coordinates(-2, -1);

            // Act
            var result = coordinates1 + coordinates2;

            // Assert
            Assert.Equal(2, result.X);
            Assert.Equal(7, result.Y);
        }

        [Fact]
        public void SubtractOperator_Subtracts()
        {
            // Arrange
            var coordinates1 = new Coordinates(4, 8);
            var coordinates2 = new Coordinates(2, 1);

            // Act
            var result = coordinates1 - coordinates2;

            // Assert
            Assert.Equal(2, result.X);
            Assert.Equal(7, result.Y);
        }
    }
}
