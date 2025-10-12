using NSubstitute;
using Solver.Components;
using Solver.Framework;

namespace SolverTests;

public class TableauExtensionsTests
{
    [Fact]
    public void Clone()
    {
        // Arrange
        var notifier = Substitute.For<INotifier>();
        var tableau = GetExampleTableau(notifier);

        // Act
        var clone = tableau.Clone();

        // Assert
        Assert.NotSame(tableau, clone);
        Assert.Same(tableau.Grid, clone.Grid);

        Assert.NotSame(tableau.Aisles, clone.Aisles);
        Assert.Equal(tableau.Aisles, clone.Aisles);

        Assert.NotSame(tableau.Tiles, clone.Tiles);
        Assert.Equal(tableau.Tiles, clone.Tiles);

        Assert.NotSame(tableau.Edges, clone.Edges);
        Assert.Equal(tableau.Edges, clone.Edges);

        Assert.NotSame(tableau.Thalweg, clone.Thalweg);
        Assert.Same(tableau.Thalweg.Grid, clone.Thalweg.Grid);
        Assert.Equal(tableau.Thalweg.TileCount, clone.Thalweg.TileCount);

        foreach (var segment in tableau.Thalweg.Segments)
        {
            Assert.NotNull(segment.First);
            Assert.True(clone.Thalweg.TryGetSegment(segment.First, out var clonedSegment));
            Assert.NotNull(clonedSegment);
            Assert.Equal(segment.Count, clonedSegment.Count);
            Assert.Equal(segment.TileCount, clonedSegment.TileCount);
            Assert.Equal(segment.TerminationCount, clonedSegment.TerminationCount);
            Assert.Equal(segment.Links, clonedSegment.Links);
        }
    }

    private static Tableau GetExampleTableau(INotifier notifier)
    {
        var factory = new TableauFactory();
        var grid = new SolverGrid(3);

        var tableau = TableauFactory.Create(
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
        Assert.True(thalweg.TryLink(edge4, notifier));

        return tableau;
    }
}
