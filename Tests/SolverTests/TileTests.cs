using NSubstitute;
using Solver.Components;
using Solver.Framework;
using Solver.Rules;

namespace SolverTests;

public class TileTests
{
    [Fact]
    public void Constructor_NullShape_Throws()
    {
        // Arrange
        var centre = new Coordinates(-1, -4, 5);

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new Tile(centre, null!));

        // Assert
        Assert.Equal("shape", ex.ParamName);
    }

    [Fact]
    public void Constructor_Up_ReturnsInstance()
    {
        // Arrange
        var centre = new Coordinates(-1, -4, 5);

        // Act
        var instance = new Tile(centre, Grid.ShapeUp);

        // Assert
        Assert.False(instance.IsFrozen);

        Assert.Equal(centre, instance.Coordinates);
        Assert.Equal(Orientation.Up, instance.Orientation);
        Assert.Equal(Resolution.Unknown, instance.Resolution);

        Assert.Equal(new Coordinates(-3, -3, 6), instance.Vertices[0]);
        Assert.Equal(new Coordinates(0, -6, 6), instance.Vertices[1]);
        Assert.Equal(new Coordinates(0, -3, 3), instance.Vertices[2]);

        Assert.Null(instance.AisleX);
        Assert.Null(instance.AisleY);
        Assert.Null(instance.AisleZ);

        Assert.Null(instance.EdgeX);
        Assert.Null(instance.EdgeY);
        Assert.Null(instance.EdgeZ);

        Assert.Equal(-196609, instance.GetHashCode());
        Assert.Equal("Tile:Up:(-1,-4,5)=>Unknown", instance.ToString());
    }

    [Fact]
    public void Constructor_Down_ReturnsInstance()
    {
        // Arrange
        var centre = new Coordinates(4, -2, -2);

        // Act
        var instance = new Tile(centre, Grid.ShapeDown);

        // Assert
        Assert.False(instance.IsFrozen);

        Assert.Equal(centre, instance.Coordinates);
        Assert.Equal(Orientation.Down, instance.Orientation);
        Assert.Equal(Resolution.Unknown, instance.Resolution);

        Assert.Equal(new Coordinates(6, -3, -3), instance.Vertices[0]);
        Assert.Equal(new Coordinates(3, 0, -3), instance.Vertices[1]);
        Assert.Equal(new Coordinates(3, -3, 0), instance.Vertices[2]);

        Assert.Null(instance.AisleX);
        Assert.Null(instance.AisleY);
        Assert.Null(instance.AisleZ);

        Assert.Null(instance.EdgeX);
        Assert.Null(instance.EdgeY);
        Assert.Null(instance.EdgeZ);

        Assert.Equal(-131068, instance.GetHashCode());
        Assert.Equal("Tile:Down:(4,-2,-2)=>Unknown", instance.ToString());
    }

    [Fact]
    public void SetAisles_WhenFrozen_Throws()
    {
        // Arrange
        var centre = new Coordinates(4, -2, -2);
        var instance = new Tile(centre, Grid.ShapeDown);

        var aisleX = new Aisle(Axis.X, 4, false, 9, 4);
        var aisleY = new Aisle(Axis.Y, 2, false, 11, 6);
        var aisleZ = new Aisle(Axis.Z, 2, false, 11, 5);

        instance.Freeze();

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            instance.SetAisles([aisleX, aisleY, aisleZ]));

        // Assert
        Assert.True(instance.IsFrozen);
        Assert.Equal("Object instance is frozen.", ex.Message);
    }

    [Fact]
    public void SetAisles_WhenUnfrozen_SetsAisles()
    {
        // Arrange
        var centre = new Coordinates(4, -2, -2);
        var instance = new Tile(centre, Grid.ShapeDown);

        var aisleX = new Aisle(Axis.X, 4, false, 9, 4);
        var aisleY = new Aisle(Axis.Y, 2, false, 11, 6);
        var aisleZ = new Aisle(Axis.Z, 2, false, 11, 5);

        // Act
        instance.SetAisles([aisleX, aisleY, aisleZ]);

        // Assert
        Assert.False(instance.IsFrozen);

        Assert.Equal(centre, instance.Coordinates);
        Assert.Equal(Orientation.Down, instance.Orientation);
        Assert.Equal(Resolution.Unknown, instance.Resolution);

        Assert.Equal(new Coordinates(6, -3, -3), instance.Vertices[0]);
        Assert.Equal(new Coordinates(3, 0, -3), instance.Vertices[1]);
        Assert.Equal(new Coordinates(3, -3, 0), instance.Vertices[2]);

        Assert.Equal(aisleX, instance.AisleX);
        Assert.Equal(aisleY, instance.AisleY);
        Assert.Equal(aisleZ, instance.AisleZ);

        Assert.Null(instance.EdgeX);
        Assert.Null(instance.EdgeY);
        Assert.Null(instance.EdgeZ);
    }

    [Fact]
    public void SetEdges_WhenUnfrozen_SetsEdges()
    {
        // Arrange
        var grid = new Grid(3);

        var centre = new Coordinates(4, -2, -2);
        var instance = new Tile(centre, Grid.ShapeDown);

        var edgeX = new Edge(instance.Vertices[1], instance.Vertices[2], grid);
        var edgeY = new Edge(instance.Vertices[2], instance.Vertices[0], grid);
        var edgeZ = new Edge(instance.Vertices[0], instance.Vertices[1], grid);

        // Act
        instance.SetEdges([edgeX, edgeY, edgeZ]);

        // Assert
        Assert.False(instance.IsFrozen);

        Assert.Equal(centre, instance.Coordinates);
        Assert.Equal(Orientation.Down, instance.Orientation);
        Assert.Equal(Resolution.Unknown, instance.Resolution);

        Assert.Equal(new Coordinates(6, -3, -3), instance.Vertices[0]);
        Assert.Equal(new Coordinates(3, 0, -3), instance.Vertices[1]);
        Assert.Equal(new Coordinates(3, -3, 0), instance.Vertices[2]);

        Assert.Null(instance.AisleX);
        Assert.Null(instance.AisleY);
        Assert.Null(instance.AisleZ);

        Assert.Equal(edgeX, instance.EdgeX);
        Assert.Equal(edgeY, instance.EdgeY);
        Assert.Equal(edgeZ, instance.EdgeZ);
    }

    [Fact]
    public void Freeze_WhenUnfrozen_Freezes()
    {
        // Arrange
        var grid = new Grid(3);

        var centre = new Coordinates(4, -2, -2);
        var instance = new Tile(centre, Grid.ShapeDown);

        var aisleX = new Aisle(Axis.X, 4, false, 9, 4);
        var aisleY = new Aisle(Axis.Y, 2, false, 11, 6);
        var aisleZ = new Aisle(Axis.Z, 2, false, 11, 5);

        var edgeX = new Edge(instance.Vertices[1], instance.Vertices[2], grid);
        var edgeY = new Edge(instance.Vertices[2], instance.Vertices[0], grid);
        var edgeZ = new Edge(instance.Vertices[0], instance.Vertices[1], grid);

        instance.SetAisles([aisleX, aisleY, aisleZ]);
        instance.SetEdges([edgeX, edgeY, edgeZ]);

        // Act
        instance.Freeze();

        // Assert
        Assert.True(instance.IsFrozen);

        Assert.Equal(centre, instance.Coordinates);
        Assert.Equal(Orientation.Down, instance.Orientation);
        Assert.Equal(Resolution.Unknown, instance.Resolution);

        Assert.Equal(new Coordinates(6, -3, -3), instance.Vertices[0]);
        Assert.Equal(new Coordinates(3, 0, -3), instance.Vertices[1]);
        Assert.Equal(new Coordinates(3, -3, 0), instance.Vertices[2]);

        Assert.Equal(aisleX, instance.AisleX);
        Assert.Equal(aisleY, instance.AisleY);
        Assert.Equal(aisleZ, instance.AisleZ);

        Assert.Equal(edgeX, instance.EdgeX);
        Assert.Equal(edgeY, instance.EdgeY);
        Assert.Equal(edgeZ, instance.EdgeZ);
    }
    [Fact]

    public void Resolve_WhenFrozen_SetsResolution()
    {
        // Arrange
        var grid = new Grid(3);
        var notifier = Substitute.For<INotifier>();

        var centre = new Coordinates(4, -2, -2);
        var instance = new Tile(centre, Grid.ShapeDown);

        var aisleX = new Aisle(Axis.X, 4, false, 9, 4);
        var aisleY = new Aisle(Axis.Y, 2, false, 11, 6);
        var aisleZ = new Aisle(Axis.Z, 2, false, 11, 5);

        var edgeX = new Edge(instance.Vertices[1], instance.Vertices[2], grid);
        var edgeY = new Edge(instance.Vertices[2], instance.Vertices[0], grid);
        var edgeZ = new Edge(instance.Vertices[0], instance.Vertices[1], grid);

        instance.SetAisles([aisleX, aisleY, aisleZ]);
        instance.SetEdges([edgeX, edgeY, edgeZ]);
        instance.Freeze();

        // Act
        var result = instance.TryResolve(Resolution.Channel, notifier, ResolutionReason.AisleCount);

        // Assert
        Assert.True(result);

        Assert.True(instance.IsFrozen);

        Assert.Equal(centre, instance.Coordinates);
        Assert.Equal(Orientation.Down, instance.Orientation);
        Assert.Equal(Resolution.Channel, instance.Resolution);

        Assert.Equal(new Coordinates(6, -3, -3), instance.Vertices[0]);
        Assert.Equal(new Coordinates(3, 0, -3), instance.Vertices[1]);
        Assert.Equal(new Coordinates(3, -3, 0), instance.Vertices[2]);

        Assert.Equal(aisleX, instance.AisleX);
        Assert.Equal(aisleY, instance.AisleY);
        Assert.Equal(aisleZ, instance.AisleZ);

        Assert.Equal(edgeX, instance.EdgeX);
        Assert.Equal(edgeY, instance.EdgeY);
        Assert.Equal(edgeZ, instance.EdgeZ);
    }
}
