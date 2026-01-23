using NSubstitute;
using Solver.Components;
using Solver.Framework;

namespace SolverTests;

public class TableauExtensionsTests
{
    [Theory]
    [InlineData(Axis.X, 0, true, 1)]
    [InlineData(Axis.X, 1, true, 2)]
    [InlineData(Axis.X, 2, false, -1)]
    [InlineData(Axis.X, 3, false, -1)]
    [InlineData(Axis.X, 4, true, 3)]
    [InlineData(Axis.X, 5, true, 4)]
    [InlineData(Axis.Y, 0, true, 1)]
    [InlineData(Axis.Y, 1, true, 2)]
    [InlineData(Axis.Y, 2, false, -1)]
    [InlineData(Axis.Y, 3, false, -1)]
    [InlineData(Axis.Y, 4, true, 3)]
    [InlineData(Axis.Y, 5, true, 4)]
    [InlineData(Axis.Z, 0, true, 1)]
    [InlineData(Axis.Z, 1, true, 2)]
    [InlineData(Axis.Z, 2, false, -1)]
    [InlineData(Axis.Z, 3, false, -1)]
    [InlineData(Axis.Z, 4, true, 3)]
    [InlineData(Axis.Z, 5, true, 4)]
    public void ProximalAisle_WithEachAisle_ReturnsExpectedValue(
        Axis axis,
        int index,
        bool expectedResult,
        int expectedIndex)
    {
        // Arrange
        var notifier = Substitute.For<INotifier>();
        var tableau = TableauProvider.GetExampleTableau(notifier);
        var aisle = tableau.Aisles[(axis, index)];

        // Act
        var result = tableau.TryGetProximalAisle(aisle, out var proximalAisle);

        // Assert
        Assert.Equal(expectedResult, result);

        if (result)
        {
            Assert.NotNull(proximalAisle);
            Assert.Equal(aisle.Axis, proximalAisle.Axis);
            Assert.Equal(expectedIndex, proximalAisle.Index);
        }
        else
        {
            Assert.Null(proximalAisle);
        }
    }

    [Theory]
    [InlineData(Axis.X, 0, false, -1)]
    [InlineData(Axis.X, 1, true, 0)]
    [InlineData(Axis.X, 2, true, 1)]
    [InlineData(Axis.X, 3, true, 4)]
    [InlineData(Axis.X, 4, true, 5)]
    [InlineData(Axis.X, 5, false, -1)]
    [InlineData(Axis.Y, 0, false, -1)]
    [InlineData(Axis.Y, 1, true, 0)]
    [InlineData(Axis.Y, 2, true, 1)]
    [InlineData(Axis.Y, 3, true, 4)]
    [InlineData(Axis.Y, 4, true, 5)]
    [InlineData(Axis.Y, 5, false, -1)]
    [InlineData(Axis.Z, 0, false, -1)]
    [InlineData(Axis.Z, 1, true, 0)]
    [InlineData(Axis.Z, 2, true, 1)]
    [InlineData(Axis.Z, 3, true, 4)]
    [InlineData(Axis.Z, 4, true, 5)]
    [InlineData(Axis.Z, 5, false, -1)]
    public void DistalAisle_WithEachAisle_ReturnsExpectedValue(
        Axis axis,
        int index,
        bool expectedResult,
        int expectedIndex)
    {
        // Arrange
        var notifier = Substitute.For<INotifier>();
        var tableau = TableauProvider.GetExampleTableau(notifier);
        var aisle = tableau.Aisles[(axis, index)];

        // Act
        var result = tableau.TryGetDistalAisle(aisle, out var distalAisle);

        // Assert
        Assert.Equal(expectedResult, result);

        if (result)
        {
            Assert.NotNull(distalAisle);
            Assert.Equal(aisle.Axis, distalAisle.Axis);
            Assert.Equal(expectedIndex, distalAisle.Index);
        }
        else
        {
            Assert.Null(distalAisle);
        }
    }
}
