using NSubstitute;
using Solver.Components;
using Solver.Framework;
using Solver.Rules;

namespace SolverTests;

public class EdgeTests
{
    [Fact]
    public void Constructor_NullGrid_Throws()
    {
        // Arrange
        var coordinates1 = new Coordinates(-3, 6, -3);
        var coordinates2 = new Coordinates(-3, 3, 0);

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new Edge(coordinates1, coordinates2, null!));

        // Assert
        Assert.Equal("grid", ex.ParamName);
    }

    [Fact]
    public void Constructor_CoordinatesEqual_Throws()
    {
        // Arrange
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(-3, 6, -3);
        var coordinates2 = new Coordinates(-3, 6, -3);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            new Edge(coordinates1, coordinates2, grid));

        // Assert
        Assert.Equal("v1,v2", ex.ParamName);
        Assert.StartsWith("Coordinates must differ: [(-3,6,-3), (-3,6,-3)].", ex.Message);
    }

    [Fact]
    public void Constructor_WithCoordinatesNormalToXAxis_ReturnsInstance()
    {
        // Act
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(-3, 6, -3);
        var coordinates2 = new Coordinates(-3, 3, 0);

        var instance = new Edge(coordinates1, coordinates2, grid);

        // Assert
        Assert.False(instance.IsFrozen);
        Assert.False(instance.IsResolved);

        Assert.Equal(Axis.X, instance.NormalAxis);
        Assert.False(instance.IsBorder);

        Assert.Equal(coordinates1, instance.Vertices[0]);
        Assert.Equal(coordinates2, instance.Vertices[1]);

        Assert.Null(instance.TileMinus);
        Assert.Null(instance.TilePlus);

        Assert.Equal(Resolution.Unknown, instance.Resolution);
        Assert.False(instance.IsResolved);

        Assert.Equal(33750656, instance.GetHashCode());
        Assert.Equal("Edge:{(-3,6,-3),(-3,3,0)}=>Unknown", instance.ToString());
    }

    [Fact]
    public void Constructor_WithCoordinatesNormalToYAxis_ReturnsInstance()
    {
        // Act
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(-6, 6, 0);
        var coordinates2 = new Coordinates(-3, 6, -3);

        var instance = new Edge(coordinates1, coordinates2, grid);

        // Assert
        Assert.False(instance.IsFrozen);
        Assert.False(instance.IsResolved);

        Assert.Equal(Axis.Y, instance.NormalAxis);
        Assert.False(instance.IsBorder);

        Assert.Equal(coordinates1, instance.Vertices[0]);
        Assert.Equal(coordinates2, instance.Vertices[1]);

        Assert.Null(instance.TileMinus);
        Assert.Null(instance.TilePlus);

        Assert.Equal(Resolution.Unknown, instance.Resolution);
        Assert.False(instance.IsResolved);

        Assert.Equal(58719491, instance.GetHashCode());
        Assert.Equal("Edge:{(-6,6,0),(-3,6,-3)}=>Unknown", instance.ToString());
    }

    [Fact]
    public void Constructor_WithCoordinatesNormalToZAxis_ReturnsInstance()
    {
        // Act
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(-3, 3, 0);
        var coordinates2 = new Coordinates(-6, 6, 0);

        var instance = new Edge(coordinates1, coordinates2, grid);

        // Assert
        Assert.False(instance.IsFrozen);
        Assert.False(instance.IsResolved);

        Assert.Equal(Axis.Z, instance.NormalAxis);
        Assert.False(instance.IsBorder);

        Assert.Equal(coordinates1, instance.Vertices[0]);
        Assert.Equal(coordinates2, instance.Vertices[1]);

        Assert.Null(instance.TileMinus);
        Assert.Null(instance.TilePlus);

        Assert.Equal(Resolution.Unknown, instance.Resolution);
        Assert.False(instance.IsResolved);

        Assert.Equal(33750653, instance.GetHashCode());
        Assert.Equal("Edge:{(-3,3,0),(-6,6,0)}=>Unknown", instance.ToString());
    }

    [Fact]
    public void Constructor_EdgeWithVerticesOutOfOrder_OrdersVertices()
    {
        // Act
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(-6, 6, 0);
        var coordinates2 = new Coordinates(-3, 3, 0);

        var instance = new Edge(coordinates1, coordinates2, grid);

        // Assert
        Assert.False(instance.IsFrozen);
        Assert.False(instance.IsResolved);

        Assert.Equal(Axis.Z, instance.NormalAxis);
        Assert.False(instance.IsBorder);

        Assert.Equal(coordinates1, instance.Vertices[0]);
        Assert.Equal(coordinates2, instance.Vertices[1]);

        Assert.Null(instance.TileMinus);
        Assert.Null(instance.TilePlus);

        Assert.Equal(33750653, instance.GetHashCode());
        Assert.Equal("Edge:{(-6,6,0),(-3,3,0)}=>Unknown", instance.ToString());
    }

    [Fact]
    public void Constructor_CoordinatesOnBorder_IsBorderTrue()
    {
        // Act
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(-9, 6, 3);
        var coordinates2 = new Coordinates(-9, 3, 6);

        var instance = new Edge(coordinates1, coordinates2, grid);

        // Assert
        Assert.False(instance.IsFrozen);
        Assert.False(instance.IsResolved);

        Assert.Equal(Axis.X, instance.NormalAxis);
        Assert.True(instance.IsBorder);

        Assert.Equal(coordinates1, instance.Vertices[0]);
        Assert.Equal(coordinates2, instance.Vertices[1]);

        Assert.Null(instance.TileMinus);
        Assert.Null(instance.TilePlus);

        Assert.Equal(33749888, instance.GetHashCode());
        Assert.Equal("Border:{(-9,6,3),(-9,3,6)}=>Unknown", instance.ToString());
    }

    [Fact]
    public void Constructor_InvalidCoordinates_Throws()
    {
        // Act
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(-6, 0, 6);
        var coordinates2 = new Coordinates(-3, 3, 0);

        var ex = Assert.Throws<ArgumentException>(() =>
            new Edge(coordinates1, coordinates2, grid));

        // Assert
        Assert.Equal("Unable to determine normal axis from coordinate pair.", ex.Message);
    }

    [Fact]
    public void Freeze_WhenUnfrozen_Freezes()
    {
        // Arrange
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(-6, 0, 6);
        var coordinates2 = new Coordinates(-3, 0, 3);

        var instance = new Edge(coordinates1, coordinates2, grid);

        // Act
        instance.Freeze();

        // Assert
        Assert.True(instance.IsFrozen);
        Assert.False(instance.IsResolved);
    }

    [Fact]
    public void SetTiles_WhenFrozen_Throws()
    {
        // Arrange
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(-6, 0, 6);
        var coordinates2 = new Coordinates(-3, 0, 3);

        var instance = new Edge(coordinates1, coordinates2, grid);
        instance.Freeze();

        var tile1 = new Tile(new Coordinates(-4, -1, 5), Grid.ShapeUp);
        var tile2 = new Tile(new Coordinates(-5, 1, 4), Grid.ShapeDown);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            instance.SetTiles(tile1, tile2));

        // Assert
        Assert.True(instance.IsFrozen);
        Assert.False(instance.IsResolved);

        Assert.Equal("Object instance is frozen.", ex.Message);
    }

    [Fact]
    public void SetTiles_WhenUnfrozen_SetsTiles()
    {
        // Arrange
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(-6, 0, 6);
        var coordinates2 = new Coordinates(-3, 0, 3);

        var instance = new Edge(coordinates1, coordinates2, grid);

        var tile1 = new Tile(new Coordinates(-4, -1, 5), Grid.ShapeUp);
        var tile2 = new Tile(new Coordinates(-5, 1, 4), Grid.ShapeDown);

        // Act
        instance.SetTiles(tile1, tile2);

        // Assert
        Assert.False(instance.IsFrozen);
        Assert.False(instance.IsResolved);

        Assert.Equal(tile1, instance.TileMinus);
        Assert.Equal(tile2, instance.TilePlus);
    }

    [Fact]
    public void SetTiles_WhenTilesInWrongOrder_SetsTilesInCorrectOrder()
    {
        // Arrange
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(-6, 0, 6);
        var coordinates2 = new Coordinates(-3, 0, 3);

        var instance = new Edge(coordinates1, coordinates2, grid);

        var tile1 = new Tile(new Coordinates(-5, 1, 4), Grid.ShapeDown);
        var tile2 = new Tile(new Coordinates(-4, -1, 5), Grid.ShapeUp);

        // Act
        instance.SetTiles(tile1, tile2);

        // Assert
        Assert.Equal(tile2, instance.TileMinus);
        Assert.Equal(tile1, instance.TilePlus);
    }

    [Fact]
    public void Resolve_WithEmptyWhenFrozen_Resolves()
    {
        // Arrange
        var grid = new Grid(3);
        var notifier = Substitute.For<INotifier>();

        var coordinates1 = new Coordinates(-6, 0, 6);
        var coordinates2 = new Coordinates(-3, 0, 3);

        var instance = new Edge(coordinates1, coordinates2, grid);
        var hashCodeBefore = instance.GetHashCode();

        var tile1 = new Tile(new Coordinates(-4, -1, 5), Grid.ShapeUp);
        var tile2 = new Tile(new Coordinates(-5, 1, 4), Grid.ShapeDown);

        instance.SetTiles(tile1, tile2);
        instance.Freeze();

        // Act
        var result = instance.TryResolve(Resolution.Empty, notifier, ResolutionReason.AisleCount);
        var hashCodeAfter = instance.GetHashCode();

        // Assert
        Assert.True(result);

        Assert.True(instance.IsResolved);
        Assert.Equal(Resolution.Empty, instance.Resolution);

        Assert.Equal(hashCodeBefore, hashCodeAfter);
        Assert.Equal("Edge:{(-6,0,6),(-3,0,3)}=>Empty", instance.ToString());
    }

    [Fact]
    public void Resolve_WithChannelWhenFrozen_Resolves()
    {
        // Arrange
        var grid = new Grid(3);
        var notifier = Substitute.For<INotifier>();

        var coordinates1 = new Coordinates(-6, 0, 6);
        var coordinates2 = new Coordinates(-3, 0, 3);

        var instance = new Edge(coordinates1, coordinates2, grid);
        var hashCodeBefore = instance.GetHashCode();

        var tile1 = new Tile(new Coordinates(-4, -1, 5), Grid.ShapeUp);
        var tile2 = new Tile(new Coordinates(-5, 1, 4), Grid.ShapeDown);

        instance.SetTiles(tile1, tile2);
        instance.Freeze();

        // Act
        var result = instance.TryResolve(Resolution.Channel, notifier, ResolutionReason.AisleCount);
        var hashCodeAfter = instance.GetHashCode();

        // Assert
        Assert.True(result);

        Assert.True(instance.IsResolved);
        Assert.Equal(Resolution.Channel, instance.Resolution);

        Assert.Equal(hashCodeBefore, hashCodeAfter);
        Assert.Equal("Edge:{(-6,0,6),(-3,0,3)}=>Channel", instance.ToString());
    }

    [Fact]
    public void IsExit_WithBorderResolvedAsChannel_ReturnsTrue()
    {
        // Arrange
        var grid = new Grid(3);
        var notifier = Substitute.For<INotifier>();

        var coordinates1 = new Coordinates(3, 6, -9);
        var coordinates2 = new Coordinates(0, 9, -9);

        var instance = new Edge(coordinates1, coordinates2, grid);

        var tile1 = (Tile?)null;
        var tile2 = new Tile(new Coordinates(1, 7, -8), Grid.ShapeDown);

        instance.SetTiles(tile1, tile2);
        instance.Freeze();

        // Act
        instance.TryResolve(Resolution.Channel, notifier, ResolutionReason.AisleCount);
        var result = instance.IsExit;

        // Assert
        Assert.True(result);

        Assert.True(instance.IsBorder);
        Assert.True(instance.IsResolved);
        Assert.Equal(Resolution.Channel, instance.Resolution);
    }

    [Fact]
    public void IsExit_WithBorderResolvedAsEmpty_ReturnsFalse()
    {
        // Arrange
        var grid = new Grid(3);
        var notifier = Substitute.For<INotifier>();

        var coordinates1 = new Coordinates(3, 6, -9);
        var coordinates2 = new Coordinates(0, 9, -9);

        var instance = new Edge(coordinates1, coordinates2, grid);

        var tile1 = (Tile?)null;
        var tile2 = new Tile(new Coordinates(1, 7, -8), Grid.ShapeDown);

        instance.SetTiles(tile1, tile2);
        instance.Freeze();

        // Act
        instance.TryResolve(Resolution.Empty, notifier, ResolutionReason.AisleCount);
        var result = instance.IsExit;

        // Assert
        Assert.False(result);

        Assert.True(instance.IsBorder);
        Assert.True(instance.IsResolved);
        Assert.Equal(Resolution.Empty, instance.Resolution);
    }

    [Fact]
    public void IsExit_WithBorderNotResolved_ReturnsNull()
    {
        // Arrange
        var grid = new Grid(3);

        var coordinates1 = new Coordinates(3, 6, -9);
        var coordinates2 = new Coordinates(0, 9, -9);

        var instance = new Edge(coordinates1, coordinates2, grid);

        var tile1 = (Tile?)null;
        var tile2 = new Tile(new Coordinates(1, 7, -8), Grid.ShapeDown);

        instance.SetTiles(tile1, tile2);
        instance.Freeze();

        // Act
        var result = instance.IsExit;

        // Assert
        Assert.Null(result);

        Assert.True(instance.IsBorder);
        Assert.False(instance.IsResolved);
        Assert.Equal(Resolution.Unknown, instance.Resolution);
    }

    [Fact]
    public void IsExit_WithInteriorEdgeResolvedAsChannel_ReturnsFalse()
    {
        // Arrange
        var grid = new Grid(3);
        var notifier = Substitute.For<INotifier>();

        var coordinates1 = new Coordinates(3, 6, -9);
        var coordinates2 = new Coordinates(0, 6, -6);

        var instance = new Edge(coordinates1, coordinates2, grid);

        var tile1 = (Tile?)null;
        var tile2 = new Tile(new Coordinates(1, 7, -8), Grid.ShapeDown);

        instance.SetTiles(tile1, tile2);
        instance.Freeze();

        // Act
        instance.TryResolve(Resolution.Channel, notifier, ResolutionReason.AisleCount);
        var result = instance.IsExit;

        // Assert
        Assert.False(result);

        Assert.False(instance.IsBorder);
        Assert.True(instance.IsResolved);
        Assert.Equal(Resolution.Channel, instance.Resolution);
    }
}
