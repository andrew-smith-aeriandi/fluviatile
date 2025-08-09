using Solver.Framework;

namespace SolverTests;

public class MathsTests
{
    [Theory]
    [InlineData(-101, 3, 1)]
    [InlineData(-4, 3, 2)]
    [InlineData(-3, 3, 0)]
    [InlineData(-2, 3, 1)]
    [InlineData(-1, 3, 2)]
    [InlineData(0, 3, 0)]
    [InlineData(1, 3, 1)]
    [InlineData(2, 3, 2)]
    [InlineData(3, 3, 0)]
    [InlineData(4, 3, 1)]
    [InlineData(101, 3, 2)]
    public void Mod_WithPositiveDivisor_ReturnsNonNegativeValue(int dividend, int divisor, int expectedResult)
    {
        // Act
        var result = Maths.Mod(dividend, divisor);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(-101, -3, -2)]
    [InlineData(-4, -3, -1)]
    [InlineData(-3, -3, 0)]
    [InlineData(-2, -3, -2)]
    [InlineData(-1, -3, -1)]
    [InlineData(0, -3, 0)]
    [InlineData(1, -3, -2)]
    [InlineData(2, -3, -1)]
    [InlineData(3, -3, 0)]
    [InlineData(4, -3, -2)]
    [InlineData(101, -3, -1)]
    public void Mod_WithNegativeDivisor_ReturnsNonPositiveValue(int dividend, int divisor, int expectedResult)
    {
        // Act
        var result = Maths.Mod(dividend, divisor);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void Mod_WithZeroDivisor_Throws()
    {
        // Act
        var ex = Assert.Throws<DivideByZeroException>(() =>
            Maths.Mod(3, 0));

        // Assert
        Assert.Equal("Attempted to divide by zero.", ex.Message);
    }
}
