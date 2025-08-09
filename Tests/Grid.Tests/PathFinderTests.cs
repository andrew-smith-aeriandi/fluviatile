using Fluviatile.Grid;
using Fluviatile.Grid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Grid.Tests
{
    public class PathFinderTests
    {
        [Fact]
        public async Task ExploreAsync()
        {
            // Arrange
            var shape = new Hexagon(1);

            var nodeCoordinates = new List<Coordinates>
            {
                new Coordinates(2, 1),
                new Coordinates(4, 2),
                new Coordinates(5, 4),
                new Coordinates(4, 5),
                new Coordinates(2, 4),
                new Coordinates(1, 2)
            };

            var aisles = new List<Aisle>
            {
                new Aisle(0, 0),
                new Aisle(1, 0),
                new Aisle(2, 0),
                new Aisle(0, 1),
                new Aisle(1, 1),
                new Aisle(2, 1),
            };

            var gridNodes = nodeCoordinates
                .Select((position, index) => new GridNode(index, position))
                .ToList();

            var terminalNodes = new List<TerminalNode>
            {
                new TerminalNode(0, new Coordinates(1, -1), new[] { (Hexagon.Azimuth180, (Node)gridNodes[0]) }, aisles[0]),
                new TerminalNode(1, new Coordinates(5, 1), new[] { (Hexagon.Azimuth240, (Node)gridNodes[1]) }, aisles[1]),
                new TerminalNode(2, new Coordinates(7, 5), new[] { (Hexagon.Azimuth300, (Node)gridNodes[2]) }, aisles[2]),
                new TerminalNode(3, new Coordinates(5, 7), new[] { (Hexagon.Azimuth000, (Node)gridNodes[3]) }, aisles[3]),
                new TerminalNode(4, new Coordinates(1, 5), new[] { (Hexagon.Azimuth060, (Node)gridNodes[4]) }, aisles[4]),
                new TerminalNode(5, new Coordinates(-1, 1), new[] { (Hexagon.Azimuth120, (Node)gridNodes[5]) }, aisles[5])
            };

            gridNodes[0].AddCounterIndexes(new int[] { 0, 1, 2 });
            gridNodes[0].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth000, terminalNodes[0]),
                (Hexagon.Azimuth120, gridNodes[1]),
                (Hexagon.Azimuth240, gridNodes[5])
            });

            gridNodes[1].AddCounterIndexes(new int[] { 3, 1, 2 });
            gridNodes[1].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth060, terminalNodes[1]),
                (Hexagon.Azimuth180, gridNodes[2]),
                (Hexagon.Azimuth300, gridNodes[0])
            });

            gridNodes[2].AddCounterIndexes(new int[] { 3, 4, 2 });
            gridNodes[2].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth000, gridNodes[1]),
                (Hexagon.Azimuth120, terminalNodes[2]),
                (Hexagon.Azimuth240, gridNodes[3])
            });

            gridNodes[3].AddCounterIndexes(new int[] { 3, 4, 5 });
            gridNodes[3].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth060, gridNodes[2]),
                (Hexagon.Azimuth180, terminalNodes[3]),
                (Hexagon.Azimuth300, gridNodes[4])
            });

            gridNodes[4].AddCounterIndexes(new int[] { 0, 4, 5 });
            gridNodes[4].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth000, gridNodes[5]),
                (Hexagon.Azimuth120, gridNodes[3]),
                (Hexagon.Azimuth240, terminalNodes[4])
            });

            gridNodes[5].AddCounterIndexes(new int[] { 0, 1, 5 });
            gridNodes[5].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth060, gridNodes[0]),
                (Hexagon.Azimuth180, gridNodes[4]),
                (Hexagon.Azimuth300, terminalNodes[5])
            });

            shape.NodeCountPermutations = new int[][]
            {
                new int[] { 0, 1, 2, 3, 4, 5 },
                new int[] { 1, 2, 3, 4, 5, 0 },
                new int[] { 2, 3, 4, 5, 0, 1 },
                new int[] { 3, 4, 5, 0, 1, 2 },
                new int[] { 4, 5, 0, 1, 2, 3 },
                new int[] { 5, 0, 1, 2, 3, 4 },
                new int[] { 3, 2, 1, 0, 5, 4 },
                new int[] { 4, 3, 2, 1, 0, 5 },
                new int[] { 5, 4, 3, 2, 1, 0 },
                new int[] { 0, 5, 4, 3, 2, 1 },
                new int[] { 1, 0, 5, 4, 3, 2 },
                new int[] { 2, 1, 0, 5, 4, 3 }
            };

            var tableau = new Tableau(shape, gridNodes, terminalNodes);
            var endPoints = new List<TerminalNode>
            {
                terminalNodes[1],
                terminalNodes[2],
                terminalNodes[3]
            };

            // Initial step
            var steps = new List<Step>
            {
                new Step(gridNodes[0], Hexagon.Azimuth180)
            };

            var pathFinderJobSpec = new PathFinderJobSpec
            {
                Tableau = tableau,
                Name = "999",
                StartPoint = terminalNodes[0],
                EndPoints = endPoints,
                ThreadCount = 1
            };

            var pathFinderJob = new PathFinderJob(pathFinderJobSpec);

            // Act
            var pathFinderState = new PathFinderState
            {
                Name = pathFinderJobSpec.Name,
                Steps = steps,
                Progress = new Progress
                {
                    RouteCount = 0,
                    ElapsedTime = TimeSpan.Zero
                }
            };

            var (uniqueSolutionCount, state) = await pathFinderJob.ExploreAsync(
                state: pathFinderState,
                cancellationToken: CancellationToken.None);

            // Assert
            Assert.Equal("999", state.Name);
            Assert.Equal(5, state.Progress.RouteCount);
            Assert.Equal(5, uniqueSolutionCount.Count);
        }
    }
}
