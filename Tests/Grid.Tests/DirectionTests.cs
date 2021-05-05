using Fluviatile.Grid;
using System;
using Xunit;

namespace Grid.Tests
{
    public class DirectionTests
    {
        [Fact]
        public void Constructor_WithValueInRange_PopulatesValue()
        {
            // Act
            var direction = new Direction(4);

            // Assert
            Assert.Equal(4, direction.Value);
        }

        [Fact]
        public void Constructor_WithValueOutOfRange_PopulatesValueModulo6()
        {
            // Act
            var direction = new Direction(-7);

            // Assert
            Assert.Equal(5, direction.Value);
        }

        [Fact]
        public void ToString_ReturnspectedString()
        {
            // Arrange
            var direction = new Direction(4);

            // Act
            var result = direction.ToString();

            // Assert
            Assert.Equal("4", result);
        }

        [Fact]
        public void AddOperator_AddTorsion_ReturnsDirection()
        {
            // Arrange
            var direction = new Direction(5);

            // Act
            var result = direction + new Torsion(9);

            // Assert
            Assert.IsType<Direction>(result);
            Assert.Equal(2, result.Value);
        }

        [Fact]
        public void SubtractOperator_SubtractTorsion_ReturnsDirection()
        {
            // Arrange
            var direction = new Direction(5);

            // Act
            var result = direction - new Torsion(10);

            // Assert
            Assert.IsType<Direction>(result);
            Assert.Equal(1, result.Value);
        }

        [Theory]
        [InlineData(5, 5, 0)]
        [InlineData(5, 4, 1)]
        [InlineData(5, 0, -1)]
        public void SubtractOperator_SubtractDirection_ReturnsTorsion(int lhs, int rhs, int expected)
        {
            // Arrange
            var direction = new Direction(lhs);

            // Act
            var result = direction - new Direction(rhs);

            // Assert
            Assert.IsType<Torsion>(result);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void SubtractOperator_SubtractDirectionWithAbsoluteTorsionGreaterThan1_Throws()
        {
            // Arrange
            var direction = new Direction(5);

            // Act + Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => direction - new Direction(3));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 3)]
        [InlineData(4, 4)]
        [InlineData(5, 5)]
        [InlineData(-9, -9)]
        [InlineData(0, 6)]
        [InlineData(2, 8)]
        [InlineData(16, -8)]
        public void Equals_WhenEqual_ReturnsTrue(int value1, int value2)
        {
            // Arrange
            var direction1 = new Direction(value1);
            var direction2 = new Direction(value2);

            // Act
            var result = direction1.Equals(direction2);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(5, -5)]
        public void Equals_WhenNotEqual_ReturnsFalse(int value1, int value2)
        {
            // Arrange
            var direction1 = new Direction(value1);
            var direction2 = new Direction(value2);

            // Act
            var result = direction1.Equals(direction2);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(-14, 4)]
        [InlineData(-1, 5)]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 3)]
        [InlineData(4, 4)]
        [InlineData(5, 5)]
        [InlineData(6, 0)]
        [InlineData(25, 1)]
        public void GetHashCode_ReturnsExpectedValue(int value, int expectedResult)
        {
            // Arrange
            var direction = new Direction(value);

            // Act
            var result = direction.GetHashCode();

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Turn_Left_ReturnsNewDirection()
        {
            // Arrange
            var direction = new Direction(0);

            // Act
            var result = direction.Turn(new Torsion(-1));

            // Assert
            Assert.Equal(5, result.Value);
        }

        [Fact]
        public void Turn_Right_ReturnsNewDirection()
        {
            // Arrange
            var direction = new Direction(0);

            // Act
            var result = direction.Turn(new Torsion(1));

            // Assert
            Assert.Equal(1, result.Value);
        }

        [Fact]
        public void Turn_Zero_ReturnsNewDirection()
        {
            // Arrange
            var direction = new Direction(0);

            // Act
            var result = direction.Turn(new Torsion(0));

            // Assert
            Assert.Equal(0, result.Value);
        }
    }
}
