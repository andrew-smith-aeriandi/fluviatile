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
                new(2, 1),
                new(4, 2),
                new(5, 4),
                new(4, 5),
                new(2, 4),
                new(1, 2)
            };

            var aisles = new List<Aisle>
            {
                new(0, 0),
                new(1, 0),
                new(2, 0),
                new(0, 1),
                new(1, 1),
                new(2, 1),
            };

            var gridNodes = nodeCoordinates
                .Select((position, index) => new GridNode(index, position))
                .ToList();

            var terminalNodes = new List<TerminalNode>
            {
                new(0, new Coordinates(1, -1), [(Hexagon.Azimuth180, (Node)gridNodes[0])], aisles[0]),
                new(1, new Coordinates(5, 1), [(Hexagon.Azimuth240, (Node)gridNodes[1])], aisles[1]),
                new(2, new Coordinates(7, 5), [(Hexagon.Azimuth300, (Node)gridNodes[2])], aisles[2]),
                new(3, new Coordinates(5, 7), [(Hexagon.Azimuth000, (Node)gridNodes[3])], aisles[3]),
                new(4, new Coordinates(1, 5), [(Hexagon.Azimuth060, (Node)gridNodes[4])], aisles[4]),
                new(5, new Coordinates(-1, 1), [(Hexagon.Azimuth120, (Node)gridNodes[5])], aisles[5])
            };

            gridNodes[0].AddCounterIndexes([0, 1, 2]);
            gridNodes[0].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth000, terminalNodes[0]),
                (Hexagon.Azimuth120, gridNodes[1]),
                (Hexagon.Azimuth240, gridNodes[5])
            });

            gridNodes[1].AddCounterIndexes([3, 1, 2]);
            gridNodes[1].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth060, terminalNodes[1]),
                (Hexagon.Azimuth180, gridNodes[2]),
                (Hexagon.Azimuth300, gridNodes[0])
            });

            gridNodes[2].AddCounterIndexes([3, 4, 2]);
            gridNodes[2].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth000, gridNodes[1]),
                (Hexagon.Azimuth120, terminalNodes[2]),
                (Hexagon.Azimuth240, gridNodes[3])
            });

            gridNodes[3].AddCounterIndexes([3, 4, 5]);
            gridNodes[3].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth060, gridNodes[2]),
                (Hexagon.Azimuth180, terminalNodes[3]),
                (Hexagon.Azimuth300, gridNodes[4])
            });

            gridNodes[4].AddCounterIndexes([0, 4, 5]);
            gridNodes[4].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth000, gridNodes[5]),
                (Hexagon.Azimuth120, gridNodes[3]),
                (Hexagon.Azimuth240, terminalNodes[4])
            });

            gridNodes[5].AddCounterIndexes([0, 1, 5]);
            gridNodes[5].AddLinks(new List<(Direction, Node)>
            {
                (Hexagon.Azimuth060, gridNodes[0]),
                (Hexagon.Azimuth180, gridNodes[4]),
                (Hexagon.Azimuth300, terminalNodes[5])
            });

            shape.NodeCountPermutations =
            [
                [0, 1, 2, 3, 4, 5],
                [1, 2, 3, 4, 5, 0],
                [2, 3, 4, 5, 0, 1],
                [3, 4, 5, 0, 1, 2],
                [4, 5, 0, 1, 2, 3],
                [5, 0, 1, 2, 3, 4],
                [3, 2, 1, 0, 5, 4],
                [4, 3, 2, 1, 0, 5],
                [5, 4, 3, 2, 1, 0],
                [0, 5, 4, 3, 2, 1],
                [1, 0, 5, 4, 3, 2],
                [2, 1, 0, 5, 4, 3]
            ];

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
                new(gridNodes[0], Hexagon.Azimuth180)
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
