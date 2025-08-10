using Solver.Components;
using Solver.Framework;
using static Solver.Framework.LinqExtensions;

namespace SolverTests;

public class LinqExtensionsTests
{
    [Fact]
    public void GetAllPairs_WithListOfIntegers_ReturnsPairs()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = items.GetAllPairs().ToList();

        // Assert
        Assert.Equal(
            [
                new UnorderedPair<int>(1, 2),
                new UnorderedPair<int>(1, 3),
                new UnorderedPair<int>(1, 4),
                new UnorderedPair<int>(1, 5),
                new UnorderedPair<int>(2, 3),
                new UnorderedPair<int>(2, 4),
                new UnorderedPair<int>(2, 5),
                new UnorderedPair<int>(3, 4),
                new UnorderedPair<int>(3, 5),
                new UnorderedPair<int>(4, 5)
            ],
            result);
    }

    [Fact]
    public void GetAllPairs_WithEmptyList_ReturnsEmpty()
    {
        // Arrange
        var items = new List<int>();

        // Act
        var result = items.GetAllPairs().ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllPairs_WithSingleItem_ReturnsEmpty()
    {
        // Arrange
        var items = new List<int>() { 1 };

        // Act
        var result = items.GetAllPairs().ToList();

        // Assert
        Assert.Empty(result);
    }
}

public class AisleTests
{
    [Fact]
    public void Constructor_NegativeIndex_Throws()
    {
        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Aisle(Axis.Z, -1, false, 9, 4));

        // Assert
        Assert.Equal("index", ex.ParamName);
        Assert.StartsWith("index ('-1') must be a non-negative value.", ex.Message);
    }

    [Fact]
    public void Constructor_NegativeTileCount_Throws()
    {
        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Aisle(Axis.Z, 1, false, -9, 4));

        // Assert
        Assert.Equal("tileCount", ex.ParamName);
        Assert.StartsWith("tileCount ('-9') must be a non-negative value.", ex.Message);
    }

    [Fact]
    public void Constructor_NegativeChannelCount_Throws()
    {
        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Aisle(Axis.Z, 1, false, 9, -4));

        // Assert
        Assert.Equal("channelCount", ex.ParamName);
        Assert.StartsWith("channelCount ('-4') must be a non-negative value.", ex.Message);
    }

    [Fact]
    public void Constructor_ChannelCountGreaterThanTileCount_Throws()
    {
        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Aisle(Axis.Z, 1, false, 9, 10));

        // Assert
        Assert.Equal("channelCount", ex.ParamName);
        Assert.StartsWith("channelCount ('10') must be less than or equal to '9'.", ex.Message);
    }

    [Fact]
    public void Constructor_ReturnsUnfrozenInstance()
    {
        // Act
        var instance = new Aisle(Axis.Z, 1, false, 9, 4);

        // Assert
        Assert.False(instance.IsFrozen);

        Assert.Equal(Axis.Z, instance.Axis);
        Assert.Equal(1, instance.Index);
        Assert.Equal(9, instance.TileCount);
        Assert.Equal(4, instance.ChannelTileCount);

        Assert.Null(instance.Tiles);

        Assert.Equal(1025, instance.GetHashCode());
        Assert.Equal("Aisle:Z[1]=>4/9", instance.ToString());
    }

    [Fact]
    public void SetTiles_WhenFrozen_Throws()
    {
        var instance = new Aisle(Axis.X, 0, true, 7, 3);
        instance.Freeze();

        var tiles = new List<Tile>
        {
            new(new Coordinates(-7, -1, 8), SolverGrid.ShapeUp),
            new(new Coordinates(-8, 1, 7), SolverGrid.ShapeDown),
            new(new Coordinates(-7, 2, 5), SolverGrid.ShapeUp),
            new(new Coordinates(-8, 4, 4), SolverGrid.ShapeDown),
            new(new Coordinates(-7, 5, 2), SolverGrid.ShapeUp),
            new(new Coordinates(-8, 7, 1), SolverGrid.ShapeDown),
            new(new Coordinates(-7, 8, -1), SolverGrid.ShapeUp)
        };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            instance.SetTiles(tiles));

        // Assert
        Assert.True(instance.IsFrozen);
        Assert.Equal("Object instance is frozen.", ex.Message);
    }

    [Fact]
    public void SetTiles_EmptyList_Throws()
    {
        var instance = new Aisle(Axis.X, 0, true, 7, 3);
        var tiles = new List<Tile>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            instance.SetTiles(tiles));

        // Assert
        Assert.Equal("tiles", ex.ParamName);
        Assert.StartsWith("Collection has 0 entries but 7 were expected.", ex.Message);
    }

    [Fact]
    public void SetTiles_InvalidLength_Throws()
    {
        var instance = new Aisle(Axis.X, 0, true, 7, 3);

        var tiles = new List<Tile>
        {
            new(new Coordinates(-7, -1, 8), SolverGrid.ShapeUp),
            new(new Coordinates(-8, 1, 7), SolverGrid.ShapeDown),
            new(new Coordinates(-7, 2, 5), SolverGrid.ShapeUp),
            new(new Coordinates(-8, 4, 4), SolverGrid.ShapeDown),
            new(new Coordinates(-7, 5, 2), SolverGrid.ShapeUp),
            new(new Coordinates(-8, 7, 1), SolverGrid.ShapeDown)
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            instance.SetTiles(tiles));

        // Assert
        Assert.Equal("tiles", ex.ParamName);
        Assert.StartsWith("Collection has 6 entries but 7 were expected.", ex.Message);
    }

    [Fact]
    public void SetTiles_ValidLength_PopulatesTilesProperty()
    {
        var grid = new SolverGrid(3);
        var instance = new Aisle(Axis.X, 5, false, 7, 3);

        var tiles = new List<Tile>
        {
            new(new Coordinates(7, -8, 1), SolverGrid.ShapeDown),
            new(new Coordinates(8, -7, -1), SolverGrid.ShapeUp),
            new(new Coordinates(7, -5, -2), SolverGrid.ShapeDown),
            new(new Coordinates(8, -4, -4), SolverGrid.ShapeUp),
            new(new Coordinates(7, -2, -5), SolverGrid.ShapeDown),
            new(new Coordinates(8, -1, -7), SolverGrid.ShapeUp),
            new(new Coordinates(7, 1, -8), SolverGrid.ShapeDown)
        };

        var edges = tiles
            .SelectMany(tile => grid.CreateEdgesFromVertices(tile.Vertices))
            .Distinct(EdgeComparer.Default)
            .ToDictionary(edge => edge.Vertices);

        foreach (var tile in tiles)
        {
            var tileEdges = tile.Vertices
                .SelectWithNext((v1, v2) => new UnorderedPair<Coordinates>(v1, v2), SelectWithNextOption.LoopBackToStart)
                .Select(pair => edges.GetValueOrDefault(pair))
                .OfType<Edge>();

            tile.SetEdges(tileEdges);
        }

        // Act
        instance.SetTiles(tiles);

        // Assert
        Assert.False(instance.IsFrozen);

        Assert.Equal(Axis.X, instance.Axis);
        Assert.Equal(5, instance.Index);
        Assert.Equal(7, instance.TileCount);
        Assert.Equal(3, instance.ChannelTileCount);

        Assert.Equal(tiles, instance.Tiles);

        Assert.Equal(261, instance.GetHashCode());
        Assert.Equal("Aisle:X[5]=>3/7", instance.ToString());
    }

    [Fact]
    public void Freeze_AfterSetTiles_FreezesInstance()
    {
        var grid = new SolverGrid(3);
        var instance = new Aisle(Axis.Y, 5, false, 7, 7);

        var tiles = new List<Tile>
        {
            new(new Coordinates(1, 7, -8), SolverGrid.ShapeDown),
            new(new Coordinates(-1, 8, -7), SolverGrid.ShapeUp),
            new(new Coordinates(-2, 7, -5), SolverGrid.ShapeDown),
            new(new Coordinates(-4, 8, -4), SolverGrid.ShapeUp),
            new(new Coordinates(-5, 7, -2), SolverGrid.ShapeDown),
            new(new Coordinates(-7, 8, -1), SolverGrid.ShapeUp),
            new(new Coordinates(-8, 7, 1), SolverGrid.ShapeDown)
        };

        var edges = tiles
            .SelectMany(tile => grid.CreateEdgesFromVertices(tile.Vertices))
            .Distinct(EdgeComparer.Default)
            .ToDictionary(edge => edge.Vertices);

        foreach (var tile in tiles)
        {
            var tileEdges = tile.Vertices
                .SelectWithNext((v1, v2) => new UnorderedPair<Coordinates>(v1, v2), SelectWithNextOption.LoopBackToStart)
                .Select(pair => edges.GetValueOrDefault(pair))
                .OfType<Edge>();

            tile.SetEdges(tileEdges);
        }

        instance.SetTiles(tiles);

        // Act
        instance.Freeze();

        // Assert
        Assert.True(instance.IsFrozen);

        Assert.Equal(Axis.Y, instance.Axis);
        Assert.Equal(5, instance.Index);
        Assert.Equal(7, instance.TileCount);
        Assert.Equal(7, instance.ChannelTileCount);

        Assert.Equal(tiles, instance.Tiles);

        Assert.Equal(517, instance.GetHashCode());
        Assert.Equal("Aisle:Y[5]=>7/7", instance.ToString());
    }
}