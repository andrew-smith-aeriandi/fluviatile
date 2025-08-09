using Solver.Framework;

namespace SolverTests;

public class CoordinatesTests
{
    [Fact]
    public void Constructor_WithXYArgs_Returns()
    {
        // Act
        var coordinates = new Coordinates(-1, -4);

        // Assert
        Assert.Equal(-1, coordinates.X);
        Assert.Equal(-4, coordinates.Y);
        Assert.Equal(5, coordinates.Z);

        Assert.Equal(-196609, coordinates.GetHashCode());
        Assert.Equal("(-1,-4,5)", coordinates.ToString());
    }

    [Fact]
    public void Constructor_WithValidXYZArgs_Returns()
    {
        // Act
        var coordinates = new Coordinates(-1, -4, 5);

        // Assert
        Assert.Equal(-1, coordinates.X);
        Assert.Equal(-4, coordinates.Y);
        Assert.Equal(5, coordinates.Z);

        Assert.Equal(-196609, coordinates.GetHashCode());
        Assert.Equal("(-1,-4,5)", coordinates.ToString());
    }

    [Fact]
    public void Constructor_WhenXYZArgsDoNotSumToZero_Throws()
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            new Coordinates(-1, -4, -5));

        // Assert
        Assert.Null(ex.ParamName);
        Assert.Equal("The sum of the coordinates must sum to zero: (-1,-4,-5)", ex.Message);
    }

    [Fact]
    public void AddOperator_WithCoordinatePair_Adds()
    {
        // Arrange
        var coordinates = new Coordinates(-1, -4, 5);
        var offset = new Coordinates(1, 1, -2);

        // Act
        var result = coordinates + offset;

        // Assert
        Assert.Equal(0, result.X);
        Assert.Equal(-3, result.Y);
        Assert.Equal(3, result.Z);
    }

    [Fact]
    public void SubtractOperator_WithCoordinatePair_Subtracts()
    {
        // Arrange
        var coordinates1 = new Coordinates(-1, -4, 5);
        var coordinates2 = new Coordinates(0, -3, 3);

        // Act
        var result = coordinates2 - coordinates1;

        // Assert
        Assert.Equal(1, result.X);
        Assert.Equal(1, result.Y);
        Assert.Equal(-2, result.Z);
    }

    [Fact]
    public void MultiplyOperator_WithCoordinateAndInteger_Multiplies()
    {
        // Arrange
        var coordinates = new Coordinates(1, -3, 2);
        var factor = 3;

        // Act
        var result = coordinates.Multiply(factor);

        // Assert
        Assert.Equal(3, result.X);
        Assert.Equal(-9, result.Y);
        Assert.Equal(6, result.Z);
    }

    [Fact]
    public void DivideOperator_WithCoordinateAndInteger_Divides()
    {
        // Arrange
        var coordinates = new Coordinates(3, -9, 6);
        var factor = 3;

        // Act
        var result = coordinates.Divide(factor);

        // Assert
        Assert.Equal(1, result.X);
        Assert.Equal(-3, result.Y);
        Assert.Equal(2, result.Z);
    }

    [Theory]
    [InlineData(3, -6, 3, -6, true)]
    [InlineData(3, -6, 4, -6, false)]
    [InlineData(3, -6, 3, -7, false)]
    public void EqualOperator_WithCoordinates_ReturnsExpectedValue(int x1, int y1, int x2, int y2, bool expectedResult)
    {
        // Arrange
        var coordinates1 = new Coordinates(x1, y1);
        var coordinates2 = new Coordinates(x2, y2);

        // Act
        var result = (coordinates1 == coordinates2);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(3, -6, 3, -6, false)]
    [InlineData(3, -6, 4, -6, true)]
    [InlineData(3, -6, 3, -7, true)]
    public void NotEqualOperator_WithCoordinates_ReturnsExpectedValue(int x1, int y1, int x2, int y2, bool expectedResult)
    {
        // Arrange
        var coordinates1 = new Coordinates(x1, y1);
        var coordinates2 = new Coordinates(x2, y2);

        // Act
        var result = (coordinates1 != coordinates2);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
