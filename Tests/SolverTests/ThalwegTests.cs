using NSubstitute;
using Solver.Components;
using Solver.Framework;

namespace SolverTests;

public class ThalwegTests
{
    [Fact]
    public void Constructor_ValidArguments_ReturnsInstance()
    {
        // Arrange
        var grid = new SolverGrid(3);

        // Act
        var thalweg = new Thalweg(grid, 29);

        // Assert
        Assert.NotNull(thalweg);

        Assert.Empty(thalweg.Segments);
        Assert.Equal(0, thalweg.SegmentCount);

        Assert.Equal(29, thalweg.TileCount);
        Assert.Equal(0, thalweg.LinkedTileCount);
        Assert.Equal(29, thalweg.UnlinkedTileCount);

        Assert.Empty(thalweg.Exits);
        Assert.Equal(2, thalweg.ExitCount);
        Assert.Equal(0, thalweg.ResolvedExitCount);
        Assert.Equal(2, thalweg.UnresolvedExitCount);

        Assert.Equal("", thalweg.ToString());
    }

    [Fact]
    public void Constructor_NullGrid_Throws()
    {
        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new Thalweg(null!, 29));

        // Assert
        Assert.Equal("grid", ex.ParamName);
    }

    [Theory]
    [InlineData(2, 25)]
    [InlineData(3, -1)]
    [InlineData(3, 55)]
    public void Constructor_InvalidTileCount_Throws(int size, int tileCount)
    {
        // Arrange
        var grid = new SolverGrid(size);

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Thalweg(grid, tileCount));

        // Assert
        Assert.Equal("tileCount", ex.ParamName);
        Assert.StartsWith($"Value must be in the range [0, {grid.TileCount}].", ex.Message);
    }

    [Fact]
    public void TryLink_UnlinkedTiles_CreatesSegment()
    {
        // Arrange
        var notifier = Substitute.For<INotifier>();
        var tableau = GetExampleTableau(notifier);
        var thalweg = tableau.Thalweg;

        var edge1 = tableau.Edges[(new Coordinates(-3, 0), new Coordinates(-6, 0))];
        edge1.TryResolve(Resolution.Channel, notifier);

        // Act
        var result = thalweg.TryLink(edge1, notifier);

        // Assert
        Assert.True(result);

        Assert.Equal(29, thalweg.TileCount);
        Assert.Equal(2, thalweg.LinkedTileCount);
        Assert.Equal(27, thalweg.UnlinkedTileCount);

        Assert.Equal(2, thalweg.ExitCount);
        Assert.Equal(0, thalweg.ResolvedExitCount);
        Assert.Equal(2, thalweg.UnresolvedExitCount);

        Assert.Equal(1, thalweg.SegmentCount);
        var segment = Assert.Single(thalweg.Segments);

        Assert.Same(thalweg, segment.Thalweg);
        Assert.Equal(2, segment.Count);
        Assert.Equal(2, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);
    }

    [Fact]
    public void TryLink_TileAndExit_CreatesSegment()
    {
        // Arrange
        var notifier = Substitute.For<INotifier>();
        var tableau = GetExampleTableau(notifier);
        var thalweg = tableau.Thalweg;

        var edge1 = tableau.Edges[(new Coordinates(6, -9), new Coordinates(9, -9))];
        edge1.TryResolve(Resolution.Channel, notifier);

        // Act
        var result = thalweg.TryLink(edge1, notifier);

        // Assert
        Assert.True(result);

        Assert.Equal(29, thalweg.TileCount);
        Assert.Equal(1, thalweg.LinkedTileCount);
        Assert.Equal(28, thalweg.UnlinkedTileCount);

        Assert.Equal(2, thalweg.ExitCount);
        Assert.Equal(1, thalweg.ResolvedExitCount);
        Assert.Equal(1, thalweg.UnresolvedExitCount);

        Assert.Equal(1, thalweg.SegmentCount);
        var segment = Assert.Single(thalweg.Segments);

        Assert.Same(thalweg, segment.Thalweg);
        Assert.Equal(2, segment.Count);
        Assert.Equal(1, segment.TileCount);
        Assert.Equal(1, segment.TerminationCount);
        Assert.Equal(0, segment.Rotation);
    }

    [Fact]
    public void TryLink_TileAndSegment_AddsTileToSegment()
    {
        // Arrange
        var notifier = Substitute.For<INotifier>();
        var tableau = GetExampleTableau(notifier);
        var thalweg = tableau.Thalweg;

        var edge1 = tableau.Edges[(new Coordinates(-3, 0), new Coordinates(-6, 0))];
        edge1.TryResolve(Resolution.Channel, notifier);

        var edge2 = tableau.Edges[(new Coordinates(-6, 0), new Coordinates(-6, 3))];
        edge2.TryResolve(Resolution.Channel, notifier);

        Assert.True(thalweg.TryLink(edge1, notifier));

        // Act
        var result = thalweg.TryLink(edge2, notifier);

        // Assert
        Assert.True(result);

        Assert.Equal(29, thalweg.TileCount);
        Assert.Equal(3, thalweg.LinkedTileCount);
        Assert.Equal(26, thalweg.UnlinkedTileCount);

        Assert.Equal(2, thalweg.ExitCount);
        Assert.Equal(0, thalweg.ResolvedExitCount);
        Assert.Equal(2, thalweg.UnresolvedExitCount);

        Assert.Equal(1, thalweg.SegmentCount);
        var segment = Assert.Single(thalweg.Segments);

        Assert.Same(thalweg, segment.Thalweg);
        Assert.Equal(3, segment.Count);
        Assert.Equal(3, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(1, segment.Rotation);
    }

    [Fact]
    public void TryLink_TwoSegments_CallsAddLastToFirst()
    {
        // Arrange
        var notifier = Substitute.For<INotifier>();
        var tableau = GetExampleTableau(notifier);
        var thalweg = tableau.Thalweg;

        var edge1 = tableau.Edges[(new Coordinates(-3, 0), new Coordinates(-6, 0))];
        edge1.TryResolve(Resolution.Channel, notifier);

        var edge2 = tableau.Edges[(new Coordinates(-6, 0), new Coordinates(-9, 3))];
        edge2.TryResolve(Resolution.Channel, notifier);

        var edge3 = tableau.Edges[(new Coordinates(-6, 0), new Coordinates(-6, 3))];
        edge3.TryResolve(Resolution.Channel, notifier);

        Assert.True(thalweg.TryLink(edge1, notifier));
        Assert.True(thalweg.TryLink(edge2, notifier));

        // Act
        var result = thalweg.TryLink(edge3, notifier);

        // Assert
        Assert.True(result);

        Assert.Equal(29, thalweg.TileCount);
        Assert.Equal(4, thalweg.LinkedTileCount);
        Assert.Equal(25, thalweg.UnlinkedTileCount);

        Assert.Equal(2, thalweg.ExitCount);
        Assert.Equal(0, thalweg.ResolvedExitCount);
        Assert.Equal(2, thalweg.UnresolvedExitCount);

        Assert.Equal(1, thalweg.SegmentCount);
        var segment = Assert.Single(thalweg.Segments);

        Assert.Same(thalweg, segment.Thalweg);
        Assert.Equal(4, segment.Count);
        Assert.Equal(4, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(2, segment.Rotation);
    }

    [Fact]
    public void TryLink_TileAndSegment_CallsAddToFirst()
    {
        // Arrange
        var notifier = Substitute.For<INotifier>();
        var tableau = GetExampleTableau(notifier);
        var thalweg = tableau.Thalweg;

        var edge1 = tableau.Edges[(new Coordinates(0, 0), new Coordinates(-3, 0))];
        edge1.TryResolve(Resolution.Channel, notifier);

        var edge2 = tableau.Edges[(new Coordinates(-3, 0), new Coordinates(0, -3))];
        edge2.TryResolve(Resolution.Channel, notifier);

        Assert.True(thalweg.TryLink(edge1, notifier));

        // Act
        var result = thalweg.TryLink(edge2, notifier);

        // Assert
        Assert.True(result);

        Assert.Equal(29, thalweg.TileCount);
        Assert.Equal(3, thalweg.LinkedTileCount);
        Assert.Equal(26, thalweg.UnlinkedTileCount);

        Assert.Equal(2, thalweg.ExitCount);
        Assert.Equal(0, thalweg.ResolvedExitCount);
        Assert.Equal(2, thalweg.UnresolvedExitCount);

        Assert.Equal(1, thalweg.SegmentCount);
        var segment = Assert.Single(thalweg.Segments);

        Assert.Same(thalweg, segment.Thalweg);
        Assert.Equal(3, segment.Count);
        Assert.Equal(3, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(1, segment.Rotation);
    }

    [Fact]
    public void TryLink_TwoSegments_CallsAddFirstToLast()
    {
        // Arrange
        var notifier = Substitute.For<INotifier>();
        var tableau = GetExampleTableau(notifier);
        var thalweg = tableau.Thalweg;

        var edge1 = tableau.Edges[(new Coordinates(-6, 3), new Coordinates(-3, 3))];
        edge1.TryResolve(Resolution.Channel, notifier);

        var edge2 = tableau.Edges[(new Coordinates(0, 0), new Coordinates(-3, 0))];
        edge2.TryResolve(Resolution.Channel, notifier);

        var edge3 = tableau.Edges[(new Coordinates(-3, 0), new Coordinates(0, -3))];
        edge3.TryResolve(Resolution.Channel, notifier);

        var edge4 = tableau.Edges[(new Coordinates(-3, 0), new Coordinates(-3, 3))];
        edge4.TryResolve(Resolution.Channel, notifier);

        Assert.True(thalweg.TryLink(edge1, notifier));
        Assert.True(thalweg.TryLink(edge2, notifier));
        Assert.True(thalweg.TryLink(edge3, notifier));

        // Act
        var result = thalweg.TryLink(edge4, notifier);

        // Assert
        Assert.True(result);

        Assert.Equal(29, thalweg.TileCount);
        Assert.Equal(5, thalweg.LinkedTileCount);
        Assert.Equal(24, thalweg.UnlinkedTileCount);

        Assert.Equal(2, thalweg.ExitCount);
        Assert.Equal(0, thalweg.ResolvedExitCount);
        Assert.Equal(2, thalweg.UnresolvedExitCount);

        Assert.Equal(1, thalweg.SegmentCount);
        var segment = Assert.Single(thalweg.Segments);

        Assert.Same(thalweg, segment.Thalweg);
        Assert.Equal(5, segment.Count);
        Assert.Equal(5, segment.TileCount);
        Assert.Equal(0, segment.TerminationCount);
        Assert.Equal(1, segment.Rotation);
    }

    [Fact]
    public void TryLink_SegmentAndExit_AddTerminationToSegment()
    {
        // Arrange
        var notifier = Substitute.For<INotifier>();
        var tableau = GetExampleTableau(notifier);
        var thalweg = tableau.Thalweg;

        var edge1 = tableau.Edges[(new Coordinates(-3, 0), new Coordinates(-6, 0))];
        edge1.TryResolve(Resolution.Channel, notifier);

        var edge2 = tableau.Edges[(new Coordinates(-6, 0), new Coordinates(-9, 3))];
        edge2.TryResolve(Resolution.Channel, notifier);

        var edge3 = tableau.Edges[(new Coordinates(-6, 0), new Coordinates(-6, 3))];
        edge3.TryResolve(Resolution.Channel, notifier);

        var edge4 = tableau.Edges[(new Coordinates(-9, 0), new Coordinates(-9, 3))];
        edge4.TryResolve(Resolution.Channel, notifier);

        Assert.True(thalweg.TryLink(edge1, notifier));
        Assert.True(thalweg.TryLink(edge2, notifier));
        Assert.True(thalweg.TryLink(edge3, notifier));

        // Act
        var result = thalweg.TryLink(edge4, notifier);

        // Assert
        Assert.True(result);

        Assert.Equal(29, thalweg.TileCount);
        Assert.Equal(4, thalweg.LinkedTileCount);
        Assert.Equal(25, thalweg.UnlinkedTileCount);

        Assert.Equal(2, thalweg.ExitCount);
        Assert.Equal(1, thalweg.ResolvedExitCount);
        Assert.Equal(1, thalweg.UnresolvedExitCount);

        Assert.Equal(1, thalweg.SegmentCount);
        var segment = Assert.Single(thalweg.Segments);

        Assert.Same(thalweg, segment.Thalweg);
        Assert.Equal(5, segment.Count);
        Assert.Equal(4, segment.TileCount);
        Assert.Equal(1, segment.TerminationCount);
        Assert.Equal(1, segment.Rotation);
    }

    private static Tableau GetExampleTableau(INotifier notifier)
    {
        var factory = new TableauFactory();
        var grid = new SolverGrid(3);

        var tableau = factory.Create(
            grid,
            [2, 5, 5, 5, 8, 4, 4, 6, 5, 7, 7, 0, 3, 6, 4, 8, 7, 1]);

        var coordinates = new List<Coordinates>
        {
            new(7, -8),
            new(5, -7),
            new(4, -8),
            new(2, -7),
            new(1, -5),
            new(2, -4),
            new(4, -5),
            new(5, -4),
            new(7, -5),
            new(8, -4),
            new(7, -2),
            new(5, -1),
            new(4, 1),
            new(5, 2),
            new(4, 4),
            new(2, 5),
            new(1, 4),
            new(-1, 5),
            new(-2, 4),
            new(-4, 5),
            new(-5, 4),
            new(-4, 2),
            new(-2, 1),
            new(-1, -1),
            new(-2, -2),
            new(-4, -1),
            new(-5, 1),
            new(-7, 2),
            new(-8, 1)
        };

        foreach (var coordinate in coordinates)
        {
            if (tableau.Tiles.TryGetValue(coordinate, out var tile))
            {
                tile.TryResolve(Resolution.Channel, notifier);
            }
        }

        return tableau;
    }
}
