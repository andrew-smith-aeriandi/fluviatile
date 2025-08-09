using Solver.Framework;

namespace SolverTests;

public class GridTests
{
    [Fact]
    public void Constructor_InvalidSize_Throws()
    {
        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Grid(0));

        // Assert
        Assert.Equal("size", ex.ParamName);
        Assert.StartsWith("Value must be between 1 and 8.", ex.Message);
    }

    [Fact]
    public void Constructor_ValidSize_ReturnsInstance()
    {
        // Act
        var instance = new Grid(3);

        // Assert
        Assert.NotNull(instance);
        Assert.Equal(3, instance.Size);

        Assert.Equal(6, instance.AisleCountPerAxis);
        Assert.Equal(18, instance.AisleCount);

        Assert.Equal(37, instance.VertexCount);
        Assert.Equal(54, instance.TileCount);
        Assert.Equal(90, instance.EdgeCount);

        Assert.Equal(3, Grid.Scale);
        Assert.Equal(9, instance.Radius);

        var expectedVertices = new List<Coordinates>
        {
            new(-9, 0),
            new(-9, 3),
            new(-9, 6),
            new(-9, 9),
            new(-6, -3),
            new(-6, 0),
            new(-6, 3),
            new(-6, 6),
            new(-6, 9),
            new(-3, -6),
            new(-3, -3),
            new(-3, 0),
            new(-3, 3),
            new(-3, 6),
            new(-3, 9),
            new(0, -9),
            new(0, -6),
            new(0, -3),
            new(0, 0),
            new(0, 3),
            new(0, 6),
            new(0, 9),
            new(3, -9),
            new(3, -6),
            new(3, -3),
            new(3, 0),
            new(3, 3),
            new(3, 6),
            new(6, -9),
            new(6, -6),
            new(6, -3),
            new(6, 0),
            new(6, 3),
            new(9, -9),
            new(9, -6),
            new(9, -3),
            new(9, 0)
        };

        Assert.Equal(
            expectedVertices,
            instance.Vertices.OrderBy(v => v.X).ThenBy(v => v.Y));

        Assert.Equal("Grid, Size:3, Scale:3", instance.ToString());
    }

    [Theory]
    [InlineData(0, false, 0)]
    [InlineData(1, false, 3)]
    [InlineData(2, false, 6)]
    [InlineData(3, false, 9)]
    [InlineData(0, true, 9)]
    [InlineData(1, true, 6)]
    [InlineData(2, true, 3)]
    [InlineData(3, true, 0)]
    public void CoordinateLength_ReturnsExpectedValue(int indexValue, bool fromEnd, int expectedResult)
    {
        // Arrange
        var grid = new Grid(3);
        var index = new Index(indexValue, fromEnd);

        // Act
        var result = grid.CoordinateLength(index);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void Axes_Enumerates_ReturnsAxes()
    {
        // Assert
        Assert.Equal([Axis.X, Axis.Y, Axis.Z], Grid.Axes);
    }

    [Theory]
    [InlineData(0, 7)]
    [InlineData(1, 9)]
    [InlineData(2, 11)]
    [InlineData(3, 11)]
    [InlineData(4, 9)]
    [InlineData(5, 7)]
    public void AisleTileCount_ValidIndex_ReturnsExpectedValue(int index, int expectedValue)
    {
        // Arrange
        var instance = new Grid(3);

        // Act
        var result = instance.AisleTileCount(index);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    public void AisleTileCount_InvalidIndex_Throws(int index)
    {
        // Arrange
        var instance = new Grid(3);

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            instance.AisleTileCount(index));

        // Assert
        Assert.Equal("index", ex.ParamName);
        Assert.StartsWith("Value mus be between 0 and 5", ex.Message);
    }

    [Theory]
    [InlineData(-7, 8, Orientation.Up)]
    [InlineData(7, 1, Orientation.Down)]
    public void GetTileOrientation_WhenCentre_Returns(int x, int y, Orientation expectedResult)
    {
        // Arrange
        var grid = new Grid(3);
        var coordinates = new Coordinates(x, y);

        // Act
        var result = grid.GetTileOrientation(coordinates);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(-6, 3)]
    public void GetTileOrientation_WhenNotCentre_Throws(int x, int y)
    {
        // Arrange
        var grid = new Grid(3);
        var coordinates = new Coordinates(x, y);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            grid.GetTileOrientation(coordinates));

        // Assert
        Assert.Equal("centre", ex.ParamName);
        Assert.StartsWith($"Invalid coordinates: {coordinates}", ex.Message);
    }

    [Theory]
    [InlineData(-10, false, -1)]
    [InlineData(-9, true, 0)]
    [InlineData(-8, true, 0)]
    [InlineData(-7, true, 0)]
    [InlineData(-6, true, 1)]
    [InlineData(-5, true, 1)]
    [InlineData(-4, true, 1)]
    [InlineData(-3, true, 2)]
    [InlineData(-2, true, 2)]
    [InlineData(-1, true, 2)]
    [InlineData(0, true, 3)]
    [InlineData(1, true, 3)]
    [InlineData(2, true, 3)]
    [InlineData(3, true, 4)]
    [InlineData(4, true, 4)]
    [InlineData(5, true, 4)]
    [InlineData(6, true, 5)]
    [InlineData(7, true, 5)]
    [InlineData(8, true, 5)]
    [InlineData(9, false, 6)]
    public void TryGetAisleIndex_ValidCoordinate_ReturnsTrue(int coordinate, bool expectedResult, int expectedIndex)
    {
        // Arrange
        var grid = new Grid(3);

        // Act
        var result = grid.TryGetAisleIndex(coordinate, out var index);

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedIndex, index);
    }

    [Fact]
    public void GetEdgeCoordinates_With3Vertices_EnumeratesEdges()
    {
        // Arrange
        var grid = new Grid(3);

        var vertices = new List<Coordinates>
        {
            new(6, -3, -3),
            new(3, 0, -3),
            new(3, -3, 0)
        };

        // Act
        var result = grid.CreateEdgesFromVertices(vertices).ToList();

        // Assert
        Assert.Equal(3, result.Count);

        Assert.Equal(vertices[0], result[0].Vertices[0]);
        Assert.Equal(vertices[1], result[0].Vertices[1]);
        Assert.Equal(Axis.Z, result[0].NormalAxis);
        Assert.False(result[0].IsFrozen);

        Assert.Equal(vertices[1], result[1].Vertices[0]);
        Assert.Equal(vertices[2], result[1].Vertices[1]);
        Assert.Equal(Axis.X, result[1].NormalAxis);
        Assert.False(result[1].IsFrozen);

        Assert.Equal(vertices[2], result[2].Vertices[0]);
        Assert.Equal(vertices[0], result[2].Vertices[1]);
        Assert.Equal(Axis.Y, result[2].NormalAxis);
        Assert.False(result[2].IsFrozen);
    }

    [Theory]
    [InlineData(-10, false)]
    [InlineData(-9, true)]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(9, true)]
    [InlineData(10, false)]
    public void InRange_WithCoordinate_ReturnsExpectedBool(int coordinate, bool expectedResult)
    {
        // Arrange
        var grid = new Grid(3);

        // Act
        var result = grid.InRange(coordinate);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(-6, 10, false)]
    [InlineData(-10, 7, false)]
    [InlineData(-9, 3, true)]
    [InlineData(-3, 6,  true)]
    [InlineData(0, 0, true)]
    [InlineData(6, -3, true)]
    [InlineData(9, -6, true)]
    [InlineData(10, -7, false)]
    [InlineData(6, -10, false)]
    public void InRange_WithCoordinates_ReturnsExpectedBool(int x, int y, bool expectedResult)
    {
        // Arrange
        var grid = new Grid(3);
        var coordinates = new Coordinates(x, y);

        // Act
        var result = grid.InRange(coordinates);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void TryGetTileCentreCoordinates_OrientationUpInRange_ReturnsTrue()
    {
        // Arrange
        var grid = new Grid(3);

        // Act
        var result = grid.TryGetTileCentreCoordinates(Orientation.Up, 4, 2, out var centre);

        // Assert
        Assert.True(result);
        Assert.Equal(new Coordinates(5, -1, -4), centre);
    }

    [Fact]
    public void TryGetTileCentreCoordinates_OrientationDownInRange_ReturnsTrue()
    {
        // Arrange
        var grid = new Grid(3);

        // Act
        var result = grid.TryGetTileCentreCoordinates(Orientation.Down, 4, 2, out var centre);

        // Assert
        Assert.True(result);
        Assert.Equal(new Coordinates(4, -2, -2), centre);
    }

    [Fact]
    public void TryGetTileCentreCoordinates_OrientationUpOutOfRange_ReturnsFalse()
    {
        // Arrange
        var grid = new Grid(3);

        // Act
        var result = grid.TryGetTileCentreCoordinates(Orientation.Up, 4, 4, out var centre);

        // Assert
        Assert.False(result);
        Assert.Equal(new Coordinates(5, 5, -10), centre);
    }

    [Fact]
    public void TryGetTileCentreCoordinates_OrientationDownOutOfRange_ReturnsTrue()
    {
        // Arrange
        var grid = new Grid(3);

        // Act
        var result = grid.TryGetTileCentreCoordinates(Orientation.Down, 6, 2, out var centre);

        // Assert
        Assert.False(result);
        Assert.Equal(new Coordinates(10, -2, -8), centre);
    }
}
