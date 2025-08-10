using Solver.Components;
using Solver.Framework;

namespace SolverTests;

public class ThalwegSegmentTests
{
    [Fact]
    public void Constructor_ZeroLinks_ReturnsInstance()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        // Act
        var instance = new Thalweg.Segment(thalweg);

        // Assert
        Assert.NotNull(instance);
        Assert.Same(thalweg, instance.Thalweg);

        Assert.Equal(0, instance.Count);
        Assert.Equal(0, instance.TileCount);
        Assert.Equal(0, instance.TerminationCount);
        Assert.Equal(0, instance.Rotation);

        Assert.Null(instance.First);
        Assert.Null(instance.Last);

        Assert.Empty(instance.Links);
        Assert.Empty(instance.Tiles);
        Assert.Empty(instance.Terminations);

        Assert.Equal("[]", instance.ToString());
    }

    [Fact]
    public void Constructor_SingleTile_ReturnsInstance()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile = new Tile(new Coordinates(-5, 1), SolverGrid.ShapeDown);

        // Act
        var instance = new Thalweg.Segment(thalweg, tile);

        // Assert
        Assert.NotNull(instance);
        Assert.Same(thalweg, instance.Thalweg);

        Assert.Equal(1, instance.Count);
        Assert.Equal(1, instance.TileCount);
        Assert.Equal(0, instance.TerminationCount);
        Assert.Equal(0, instance.Rotation);

        Assert.Same(tile, instance.First);
        Assert.Same(tile, instance.Last);

        Assert.Equal([tile], instance.Links);
        Assert.Equal([tile], instance.Tiles);
        Assert.Empty(instance.Terminations);

        Assert.Equal("[?, (-5,1,4), ?]", instance.ToString());
    }

    [Fact]
    public void Constructor_SingleTermination_ReturnsInstance()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var termination = new Termination(
            new Coordinates(8, -10),
            new Edge(new Coordinates(6, -9), new Coordinates(9, -9), grid));

        // Act
        var instance = new Thalweg.Segment(thalweg, termination);

        // Assert
        Assert.NotNull(instance);
        Assert.Same(thalweg, instance.Thalweg);

        Assert.Equal(1, instance.Count);
        Assert.Equal(0, instance.TileCount);
        Assert.Equal(1, instance.TerminationCount);
        Assert.Equal(0, instance.Rotation);

        Assert.Same(termination, instance.First);
        Assert.Same(termination, instance.Last);

        Assert.Equal([termination], instance.Links);
        Assert.Empty(instance.Tiles);
        Assert.Equal([termination], instance.Terminations);

        Assert.Equal("[]", instance.ToString());
    }

    [Fact]
    public void Constructor_TwoTiles_ReturnsInstance()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(-5, 1), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(-4, -1), SolverGrid.ShapeUp);

        // Act
        var instance = new Thalweg.Segment(thalweg, tile1, tile2);

        // Assert
        Assert.NotNull(instance);
        Assert.Same(thalweg, instance.Thalweg);

        Assert.Equal(2, instance.Count);
        Assert.Equal(2, instance.TileCount);
        Assert.Equal(0, instance.TerminationCount);
        Assert.Equal(0, instance.Rotation);

        Assert.Same(tile1, instance.First);
        Assert.Same(tile2, instance.Last);

        Assert.Equal([tile1, tile2], instance.Links);
        Assert.Equal([tile1, tile2], instance.Tiles);
        Assert.Empty(instance.Terminations);

        Assert.Equal("[?, (-5,1,4), (-4,-1,5), ?]", instance.ToString());
    }

    [Fact]
    public void Constructor_ThreeTiles_ReturnsInstance()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(-5, 1), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(-4, -1), SolverGrid.ShapeUp);
        var tile3 = new Tile(new Coordinates(-2, -2), SolverGrid.ShapeDown);

        // Act
        var instance = new Thalweg.Segment(thalweg, tile1, tile2, tile3);

        // Assert
        Assert.NotNull(instance);
        Assert.Same(thalweg, instance.Thalweg);

        Assert.Equal(3, instance.Count);
        Assert.Equal(3, instance.TileCount);
        Assert.Equal(0, instance.TerminationCount);
        Assert.Equal(1, instance.Rotation);

        Assert.Same(tile1, instance.First);
        Assert.Same(tile3, instance.Last);

        Assert.Equal([tile1, tile2, tile3], instance.Links);
        Assert.Equal([tile1, tile2, tile3], instance.Tiles);
        Assert.Empty(instance.Terminations);

        Assert.Equal("[?, (-5,1,4), (-4,-1,5), (-2,-2,4), ?]", instance.ToString());
    }

    [Fact]
    public void Constructor_TileAndTermination_ReturnsInstance()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);

        var termination = new Termination(
            new Coordinates(8, -10),
            new Edge(new Coordinates(6, -9), new Coordinates(9, -9), grid));

        // Act
        var instance = new Thalweg.Segment(thalweg, tile, termination);

        // Assert
        Assert.NotNull(instance);
        Assert.Same(thalweg, instance.Thalweg);

        Assert.Equal(2, instance.Count);
        Assert.Equal(1, instance.TileCount);
        Assert.Equal(1, instance.TerminationCount);
        Assert.Equal(0, instance.Rotation);

        Assert.Same(tile, instance.First);
        Assert.Same(termination, instance.Last);

        Assert.Equal([tile, termination], instance.Links);
        Assert.Equal([tile], instance.Tiles);
        Assert.Equal([termination], instance.Terminations);

        Assert.Equal("[?, (7,-8,1)]", instance.ToString());
    }

    [Fact]
    public void Constructor_TerminationAndTile_ReturnsInstance()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var termination = new Termination(
            new Coordinates(8, -10),
            new Edge(new Coordinates(6, -9), new Coordinates(9, -9), grid));

        var tile = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);

        // Act
        var instance = new Thalweg.Segment(thalweg, termination, tile);

        // Assert
        Assert.NotNull(instance);
        Assert.Same(thalweg, instance.Thalweg);

        Assert.Equal(2, instance.Count);
        Assert.Equal(1, instance.TileCount);
        Assert.Equal(1, instance.TerminationCount);
        Assert.Equal(0, instance.Rotation);

        Assert.Same(termination, instance.First);
        Assert.Same(tile, instance.Last);

        Assert.Equal([termination, tile], instance.Links);
        Assert.Equal([tile], instance.Tiles);
        Assert.Equal([termination], instance.Terminations);

        Assert.Equal("[(7,-8,1), ?]", instance.ToString());
    }

    [Fact]
    public void Constructor_AllLinks_ReturnsInstance()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var termination1 = new Termination(
            new Coordinates(8, -10),
            new Edge(new Coordinates(6, -9), new Coordinates(9, -9), grid));

        var termination2 = new Termination(
            new Coordinates(-10, 2),
            new Edge(new Coordinates(-9, 0), new Coordinates(-9, 3), grid));

        var tiles = new List<Tile>
        {
            new(new Coordinates(7, -8), SolverGrid.ShapeDown),
            new(new Coordinates(5, -7), SolverGrid.ShapeUp),
            new(new Coordinates(4, -8), SolverGrid.ShapeDown),
            new(new Coordinates(2, -7), SolverGrid.ShapeUp),
            new(new Coordinates(1, -5), SolverGrid.ShapeDown),
            new(new Coordinates(2, -4), SolverGrid.ShapeUp),
            new(new Coordinates(4, -5), SolverGrid.ShapeDown),
            new(new Coordinates(5, -4), SolverGrid.ShapeUp),
            new(new Coordinates(7, -5), SolverGrid.ShapeDown),
            new(new Coordinates(8, -4), SolverGrid.ShapeUp),
            new(new Coordinates(7, -2), SolverGrid.ShapeDown),
            new(new Coordinates(5, -1), SolverGrid.ShapeUp),
            new(new Coordinates(4, 1), SolverGrid.ShapeDown),
            new(new Coordinates(5, 2), SolverGrid.ShapeUp),
            new(new Coordinates(4, 4), SolverGrid.ShapeDown),
            new(new Coordinates(2, 5), SolverGrid.ShapeUp),
            new(new Coordinates(1, 4), SolverGrid.ShapeDown),
            new(new Coordinates(-1, 5), SolverGrid.ShapeUp),
            new(new Coordinates(-2, 4), SolverGrid.ShapeDown),
            new(new Coordinates(-4, 5), SolverGrid.ShapeUp),
            new(new Coordinates(-5, 4), SolverGrid.ShapeDown),
            new(new Coordinates(-4, 2), SolverGrid.ShapeUp),
            new(new Coordinates(-2, 1), SolverGrid.ShapeDown),
            new(new Coordinates(-1, -1), SolverGrid.ShapeUp),
            new(new Coordinates(-2, -2), SolverGrid.ShapeDown),
            new(new Coordinates(-4, -1), SolverGrid.ShapeUp),
            new(new Coordinates(-5, 1), SolverGrid.ShapeDown),
            new(new Coordinates(-7, 2), SolverGrid.ShapeUp),
            new(new Coordinates(-8, 1), SolverGrid.ShapeDown)
        };

        // Act
        var instance = new Thalweg.Segment(thalweg, [termination1, .. tiles, termination2]);

        // Assert
        Assert.NotNull(instance);
        Assert.Same(thalweg, instance.Thalweg);

        Assert.Equal(31, instance.Count);
        Assert.Equal(29, instance.TileCount);
        Assert.Equal(2, instance.TerminationCount);
        Assert.Equal(1, instance.Rotation);

        Assert.Same(termination1, instance.First);
        Assert.Same(termination2, instance.Last);

        Assert.Equal([termination1, .. tiles, termination2], instance.Links);
        Assert.Equal(tiles, instance.Tiles);
        Assert.Equal([termination1, termination2], instance.Terminations);

        Assert.Equal(
            "[(7,-8,1), (5,-7,2), (4,-8,4), (2,-7,5), (1,-5,4), (2,-4,2), (4,-5,1), (5,-4,-1), (7,-5,-2), (8,-4,-4), (7,-2,-5), (5,-1,-4), (4,1,-5), (5,2,-7), (4,4,-8), (2,5,-7), (1,4,-5), (-1,5,-4), (-2,4,-2), (-4,5,-1), (-5,4,1), (-4,2,2), (-2,1,1), (-1,-1,2), (-2,-2,4), (-4,-1,5), (-5,1,4), (-7,2,5), (-8,1,7)]",
            instance.ToString());
    }

    [Fact]
    public void AddToFirst_NullLink_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);
        var segment = new Thalweg.Segment(thalweg);

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            segment.AddToFirst(null!));

        // Assert
        Assert.Equal("link", ex.ParamName);
    }

    [Fact]
    public void AddToFirst_CannotAddLinkToTermination_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var termination = new Termination(
            new Coordinates(8, -10),
            new Edge(new Coordinates(6, -9), new Coordinates(9, -9), grid));

        var tile = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, termination, tile);
        var invalidTile = new Tile(new Coordinates(10, -11), SolverGrid.ShapeDown);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            segment.AddToFirst(invalidTile));

        // Assert
        Assert.Equal("Cannot add a link to a termination.", ex.Message);
    }

    [Fact]
    public void AddToFirst_LinkAlreadyInSegment_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(5, -7), SolverGrid.ShapeUp);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            segment.AddToFirst(tile1));

        // Assert
        Assert.Equal("Link is already in a segment.", ex.Message);
    }

    [Fact]
    public void AddToFirst_TileWithZeroLinks_AddsLink()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);
        var segment = new Thalweg.Segment(thalweg);

        var tile = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);

        // Act
        segment.AddToFirst(tile);

        // Assert
        Assert.Equal(1, segment.Count);
        Assert.Equal(1, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);

        Assert.Same(tile, segment.First);
        Assert.Same(tile, segment.Last);

        Assert.Equal([tile], segment.Links);
        Assert.Equal([tile], segment.Tiles);
        Assert.Empty(segment.Terminations);
    }

    [Fact]
    public void AddToFirst_TerminationWithZeroLinks_AddsLink()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);
        var segment = new Thalweg.Segment(thalweg);

        var termination = new Termination(
            new Coordinates(8, -10),
            new Edge(new Coordinates(6, -9), new Coordinates(9, -9), grid));

        // Act
        segment.AddToFirst(termination);

        // Assert
        Assert.Equal(1, segment.Count);
        Assert.Equal(0, segment.TileCount);
        Assert.Equal(1, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);

        Assert.Same(termination, segment.First);
        Assert.Same(termination, segment.Last);

        Assert.Equal([termination], segment.Links);
        Assert.Empty(segment.Tiles);
        Assert.Equal([termination], segment.Terminations);
    }

    [Fact]
    public void AddToFirst_TileWithSingleTileSegment_AddsLink()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(5, -7), SolverGrid.ShapeUp);

        var segment = new Thalweg.Segment(thalweg, tile2);

        // Act
        segment.AddToFirst(tile1);

        // Assert
        Assert.Equal(2, segment.Count);
        Assert.Equal(2, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile2, segment.Last);

        Assert.Equal([tile1, tile2], segment.Links);
        Assert.Equal([tile1, tile2], segment.Tiles);
        Assert.Empty(segment.Terminations);
    }

    [Fact]
    public void AddToFirst_TileWithTwoTileSegment_AddsLink()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(5, -7), SolverGrid.ShapeUp);
        var tile3 = new Tile(new Coordinates(4, -8), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile2, tile1);

        // Act
        segment.AddToFirst(tile3);

        // Assert
        Assert.Equal(3, segment.Count);
        Assert.Equal(3, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(-1, segment.Rotation);

        Assert.Same(tile3, segment.First);
        Assert.Same(tile1, segment.Last);

        Assert.Equal([tile3, tile2, tile1], segment.Links);
        Assert.Equal([tile3, tile2, tile1], segment.Tiles);
        Assert.Empty(segment.Terminations);
    }

    [Fact]
    public void AddToLast_NullLink_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);
        var segment = new Thalweg.Segment(thalweg);

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            segment.AddToLast(null!));

        // Assert
        Assert.Equal("link", ex.ParamName);
    }

    [Fact]
    public void AddToLast_CannotAddLinkToTermination_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var termination = new Termination(
            new Coordinates(8, -10),
            new Edge(new Coordinates(6, -9), new Coordinates(9, -9), grid));

        var tile = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile, termination);
        var invalidTile = new Tile(new Coordinates(10, -11), SolverGrid.ShapeDown);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            segment.AddToLast(invalidTile));

        // Assert
        Assert.Equal("Cannot add a link to a termination.", ex.Message);
    }

    [Fact]
    public void AddToLast_LinkAlreadyInSegment_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(5, -7), SolverGrid.ShapeUp);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            segment.AddToLast(tile1));

        // Assert
        Assert.Equal("Link is already in a segment.", ex.Message);
    }

    [Fact]
    public void AddToLast_TileWithZeroLinks_AddsLink()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);
        var segment = new Thalweg.Segment(thalweg);

        var tile = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);

        // Act
        segment.AddToLast(tile);

        // Assert
        Assert.Equal(1, segment.Count);
        Assert.Equal(1, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);

        Assert.Same(tile, segment.First);
        Assert.Same(tile, segment.Last);

        Assert.Equal([tile], segment.Links);
        Assert.Equal([tile], segment.Tiles);
        Assert.Empty(segment.Terminations);
    }

    [Fact]
    public void AddToLast_TerminationWithZeroLinks_AddsLink()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);
        var segment = new Thalweg.Segment(thalweg);

        var termination = new Termination(
            new Coordinates(8, -10),
            new Edge(new Coordinates(6, -9), new Coordinates(9, -9), grid));

        // Act
        segment.AddToLast(termination);

        // Assert
        Assert.Equal(1, segment.Count);
        Assert.Equal(0, segment.TileCount);
        Assert.Equal(1, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);

        Assert.Same(termination, segment.First);
        Assert.Same(termination, segment.Last);

        Assert.Equal([termination], segment.Links);
        Assert.Empty(segment.Tiles);
        Assert.Equal([termination], segment.Terminations);
    }

    [Fact]
    public void AddToLast_TileWithSingleTileSegment_AddsLink()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(5, -7), SolverGrid.ShapeUp);

        var segment = new Thalweg.Segment(thalweg, tile1);

        // Act
        segment.AddToLast(tile2);

        // Assert
        Assert.Equal(2, segment.Count);
        Assert.Equal(2, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile2, segment.Last);

        Assert.Equal([tile1, tile2], segment.Links);
        Assert.Equal([tile1, tile2], segment.Tiles);
        Assert.Empty(segment.Terminations);
    }

    [Fact]
    public void AddToLast_TileWithTwoTileSegment_AddsLink()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(7, -8), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(5, -7), SolverGrid.ShapeUp);
        var tile3 = new Tile(new Coordinates(4, -8), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);

        // Act
        segment.AddToLast(tile3);

        // Assert
        Assert.Equal(3, segment.Count);
        Assert.Equal(3, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(1, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile3, segment.Last);

        Assert.Equal([tile1, tile2, tile3], segment.Links);
        Assert.Equal([tile1, tile2, tile3], segment.Tiles);
        Assert.Empty(segment.Terminations);
    }

    [Fact]
    public void AddLastToFirst_WithNullSegment_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            segment.AddLastToFirst(null!));

        // Assert
        Assert.Equal("segment", ex.ParamName);
    }

    [Fact]
    public void AddLastToFirst_WithSameSegment_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            segment.AddLastToFirst(segment));

        // Assert
        Assert.Equal("segment", ex.ParamName);
        Assert.StartsWith("Cannot join segment to itself.", ex.Message);
    }

    [Fact]
    public void AddLastToFirst_CannotJoinSegmentsAtTermination_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);
        var tile3 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);

        var termination = new Termination(
            new Coordinates(5, 5),
            new Edge(new Coordinates(3, 6), new Coordinates(6, 3), grid));

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);
        var otherSegment = new Thalweg.Segment(thalweg, tile3, termination);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            segment.AddLastToFirst(otherSegment));

        // Assert
        Assert.Equal("Cannot join segments at a termination.", ex.Message);
    }

    [Fact]
    public void AddLastToFirst_WithEmptySegment_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);
        var otherSegment = new Thalweg.Segment(thalweg);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddLastToFirst(otherSegment);

        // Assert
        Assert.Equal(2, segment.Count);
        Assert.Equal(2, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile2, segment.Last);

        Assert.Equal([tile1, tile2], segment.Links);
        Assert.Equal([tile1, tile2], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddLastToFirst_WhenCurrentSegmentIsEmpty_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile3 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg);
        var otherSegment = new Thalweg.Segment(thalweg, tile1, tile2, tile3);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddLastToFirst(otherSegment);

        // Assert
        Assert.Equal(3, segment.Count);
        Assert.Equal(3, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(1, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile3, segment.Last);

        Assert.Equal([tile1, tile2, tile3], segment.Links);
        Assert.Equal([tile1, tile2, tile3], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddLastToFirst_WithTwoTileSegments_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(2, 5), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);
        var tile3 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile4 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var otherSegment = new Thalweg.Segment(thalweg, tile1, tile2);
        var segment = new Thalweg.Segment(thalweg, tile3, tile4);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddLastToFirst(otherSegment);

        // Assert
        Assert.Equal(4, segment.Count);
        Assert.Equal(4, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(-2, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile4, segment.Last);

        Assert.Equal([tile1, tile2, tile3, tile4], segment.Links);
        Assert.Equal([tile1, tile2, tile3, tile4], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddFirstToFirst_WithNullSegment_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            segment.AddFirstToFirst(null!));

        // Assert
        Assert.Equal("segment", ex.ParamName);
    }

    [Fact]
    public void AddFirstToFirst_WithSameSegment_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            segment.AddFirstToFirst(segment));

        // Assert
        Assert.Equal("segment", ex.ParamName);
        Assert.StartsWith("Cannot join segment to itself.", ex.Message);
    }

    [Fact]
    public void AddFirstToFirst_CannotJoinSegmentsAtTermination_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);
        var tile3 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);

        var termination = new Termination(
            new Coordinates(5, 5),
            new Edge(new Coordinates(3, 6), new Coordinates(6, 3), grid));

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);
        var otherSegment = new Thalweg.Segment(thalweg, termination, tile3);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            segment.AddFirstToFirst(otherSegment));

        // Assert
        Assert.Equal("Cannot join segments at a termination.", ex.Message);
    }

    [Fact]
    public void AddFirstToFirst_WithEmptySegment_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);
        var otherSegment = new Thalweg.Segment(thalweg);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddFirstToFirst(otherSegment);

        // Assert
        Assert.Equal(2, segment.Count);
        Assert.Equal(2, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile2, segment.Last);

        Assert.Equal([tile1, tile2], segment.Links);
        Assert.Equal([tile1, tile2], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddFirstToFirst_WhenCurrentSegmentIsEmpty_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile3 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg);
        var otherSegment = new Thalweg.Segment(thalweg, tile1, tile2, tile3);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddFirstToFirst(otherSegment);

        // Assert
        Assert.Equal(3, segment.Count);
        Assert.Equal(3, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(-1, segment.Rotation);

        Assert.Same(tile3, segment.First);
        Assert.Same(tile1, segment.Last);

        Assert.Equal([tile3, tile2, tile1], segment.Links);
        Assert.Equal([tile3, tile2, tile1], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddFirstToFirst_WithTwoTileSegments_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(2, 5), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);
        var tile3 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile4 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var otherSegment = new Thalweg.Segment(thalweg, tile2, tile1);
        var segment = new Thalweg.Segment(thalweg, tile3, tile4);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddFirstToFirst(otherSegment);

        // Assert
        Assert.Equal(4, segment.Count);
        Assert.Equal(4, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(-2, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile4, segment.Last);

        Assert.Equal([tile1, tile2, tile3, tile4], segment.Links);
        Assert.Equal([tile1, tile2, tile3, tile4], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddFirstToLast_WithNullSegment_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            segment.AddFirstToLast(null!));

        // Assert
        Assert.Equal("segment", ex.ParamName);
    }

    [Fact]
    public void AddFirstToLast_WithSameSegment_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            segment.AddFirstToLast(segment));

        // Assert
        Assert.Equal("segment", ex.ParamName);
        Assert.StartsWith("Cannot join segment to itself.", ex.Message);
    }

    [Fact]
    public void AddFirstToLast_CannotJoinSegmentsAtTermination_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);
        var tile3 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);

        var termination = new Termination(
            new Coordinates(5, 5),
            new Edge(new Coordinates(3, 6), new Coordinates(6, 3), grid));

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);
        var otherSegment = new Thalweg.Segment(thalweg, termination, tile3);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            segment.AddFirstToLast(otherSegment));

        // Assert
        Assert.Equal("Cannot join segments at a termination.", ex.Message);
    }

    [Fact]
    public void AddFirstToLast_WithEmptySegment_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);
        var otherSegment = new Thalweg.Segment(thalweg);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddFirstToLast(otherSegment);

        // Assert
        Assert.Equal(2, segment.Count);
        Assert.Equal(2, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile2, segment.Last);

        Assert.Equal([tile1, tile2], segment.Links);
        Assert.Equal([tile1, tile2], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddFirstToLast_WhenCurrentSegmentIsEmpty_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile3 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg);
        var otherSegment = new Thalweg.Segment(thalweg, tile1, tile2, tile3);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddFirstToLast(otherSegment);

        // Assert
        Assert.Equal(3, segment.Count);
        Assert.Equal(3, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(1, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile3, segment.Last);

        Assert.Equal([tile1, tile2, tile3], segment.Links);
        Assert.Equal([tile1, tile2, tile3], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddFirstToLast_WithTwoTileSegments_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(2, 5), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);
        var tile3 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile4 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);
        var otherSegment = new Thalweg.Segment(thalweg, tile3, tile4);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddFirstToLast(otherSegment);

        // Assert
        Assert.Equal(4, segment.Count);
        Assert.Equal(4, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(-2, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile4, segment.Last);

        Assert.Equal([tile1, tile2, tile3, tile4], segment.Links);
        Assert.Equal([tile1, tile2, tile3, tile4], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddLastToLast_WithNullSegment_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            segment.AddLastToLast(null!));

        // Assert
        Assert.Equal("segment", ex.ParamName);
    }

    [Fact]
    public void AddLastToLast_WithSameSegment_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            segment.AddLastToLast(segment));

        // Assert
        Assert.Equal("segment", ex.ParamName);
        Assert.StartsWith("Cannot join segment to itself.", ex.Message);
    }

    [Fact]
    public void AddLastToLast_CannotJoinSegmentsAtTermination_Throws()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);
        var tile3 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);

        var termination = new Termination(
            new Coordinates(5, 5),
            new Edge(new Coordinates(3, 6), new Coordinates(6, 3), grid));

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);
        var otherSegment = new Thalweg.Segment(thalweg, tile3, termination);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() =>
            segment.AddLastToLast(otherSegment));

        // Assert
        Assert.Equal("Cannot join segments at a termination.", ex.Message);
    }

    [Fact]
    public void AddLastToLast_WithEmptySegment_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);
        var otherSegment = new Thalweg.Segment(thalweg);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddLastToLast(otherSegment);

        // Assert
        Assert.Equal(2, segment.Count);
        Assert.Equal(2, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile2, segment.Last);

        Assert.Equal([tile1, tile2], segment.Links);
        Assert.Equal([tile1, tile2], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddLastToLast_WhenCurrentSegmentIsEmpty_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);
        var tile2 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile3 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg);
        var otherSegment = new Thalweg.Segment(thalweg, tile1, tile2, tile3);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddLastToLast(otherSegment);

        // Assert
        Assert.Equal(3, segment.Count);
        Assert.Equal(3, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(-1, segment.Rotation);

        Assert.Same(tile3, segment.First);
        Assert.Same(tile1, segment.Last);

        Assert.Equal([tile3, tile2, tile1], segment.Links);
        Assert.Equal([tile3, tile2, tile1], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddLastToLast_WithTwoTileSegments_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var tile1 = new Tile(new Coordinates(2, 5), SolverGrid.ShapeUp);
        var tile2 = new Tile(new Coordinates(4, 4), SolverGrid.ShapeDown);
        var tile3 = new Tile(new Coordinates(5, 2), SolverGrid.ShapeUp);
        var tile4 = new Tile(new Coordinates(4, 1), SolverGrid.ShapeDown);

        var segment = new Thalweg.Segment(thalweg, tile1, tile2);
        var otherSegment = new Thalweg.Segment(thalweg, tile4, tile3);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddLastToLast(otherSegment);

        // Assert
        Assert.Equal(4, segment.Count);
        Assert.Equal(4, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(-2, segment.Rotation);

        Assert.Same(tile1, segment.First);
        Assert.Same(tile4, segment.Last);

        Assert.Equal([tile1, tile2, tile3, tile4], segment.Links);
        Assert.Equal([tile1, tile2, tile3, tile4], segment.Tiles);
        Assert.Empty(segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }

    [Fact]
    public void AddLastToLast_TwoSegmentsWithAllTiles_MergesSegments()
    {
        // Arrange
        var grid = new SolverGrid(3);
        var thalweg = new Thalweg(grid, 29);

        var termination1 = new Termination(
            new Coordinates(8, -10),
            new Edge(new Coordinates(6, -9), new Coordinates(9, -9), grid));

        var tiles1 = new List<Tile>
        {
            new(new Coordinates(7, -8), SolverGrid.ShapeDown),
            new(new Coordinates(5, -7), SolverGrid.ShapeUp),
            new(new Coordinates(4, -8), SolverGrid.ShapeDown),
            new(new Coordinates(2, -7), SolverGrid.ShapeUp),
            new(new Coordinates(1, -5), SolverGrid.ShapeDown),
            new(new Coordinates(2, -4), SolverGrid.ShapeUp),
            new(new Coordinates(4, -5), SolverGrid.ShapeDown),
            new(new Coordinates(5, -4), SolverGrid.ShapeUp),
            new(new Coordinates(7, -5), SolverGrid.ShapeDown),
            new(new Coordinates(8, -4), SolverGrid.ShapeUp),
            new(new Coordinates(7, -2), SolverGrid.ShapeDown),
            new(new Coordinates(5, -1), SolverGrid.ShapeUp),
            new(new Coordinates(4, 1), SolverGrid.ShapeDown),
            new(new Coordinates(5, 2), SolverGrid.ShapeUp),
            new(new Coordinates(4, 4), SolverGrid.ShapeDown),
            new(new Coordinates(2, 5), SolverGrid.ShapeUp),
            new(new Coordinates(1, 4), SolverGrid.ShapeDown)
        };

        var termination2 = new Termination(
            new Coordinates(-10, 2),
            new Edge(new Coordinates(-9, 0), new Coordinates(-9, 3), grid));

        var tiles2 = new List<Tile>
        {
            new(new Coordinates(-8, 1), SolverGrid.ShapeDown),
            new(new Coordinates(-7, 2), SolverGrid.ShapeUp),
            new(new Coordinates(-5, 1), SolverGrid.ShapeDown),
            new(new Coordinates(-4, -1), SolverGrid.ShapeUp),
            new(new Coordinates(-2, -2), SolverGrid.ShapeDown),
            new(new Coordinates(-1, -1), SolverGrid.ShapeUp),
            new(new Coordinates(-2, 1), SolverGrid.ShapeDown),
            new(new Coordinates(-4, 2), SolverGrid.ShapeUp),
            new(new Coordinates(-5, 4), SolverGrid.ShapeDown),
            new(new Coordinates(-4, 5), SolverGrid.ShapeUp),
            new(new Coordinates(-2, 4), SolverGrid.ShapeDown),
            new(new Coordinates(-1, 5), SolverGrid.ShapeUp)
        };

        var segment = new Thalweg.Segment(thalweg, [termination1, .. tiles1]);
        var otherSegment = new Thalweg.Segment(thalweg, [termination2, .. tiles2]);
        Assert.Equal(2, thalweg.SegmentCount);

        // Act
        segment.AddLastToLast(otherSegment);

        // Assert
        Assert.Equal(31, segment.Count);
        Assert.Equal(29, segment.TileCount);
        Assert.Equal(2, segment.TerminationCount);
        Assert.Equal(1, segment.Rotation);

        Assert.Same(termination1, segment.First);
        Assert.Same(termination2, segment.Last);

        var tiles2Reversed = tiles2.AsEnumerable().Reverse().ToList();
        Assert.Equal([termination1, .. tiles1, .. tiles2Reversed, termination2], segment.Links);
        Assert.Equal([.. tiles1, .. tiles2Reversed], segment.Tiles);
        Assert.Equal([termination1, termination2], segment.Terminations);

        Assert.Equal(1, thalweg.SegmentCount);
    }
}
