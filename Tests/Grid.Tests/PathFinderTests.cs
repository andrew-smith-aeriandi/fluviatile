using Fluviatile.Grid;
using System;
using System.Collections.Generic;
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

            var nodes = new List<Node>
            {
                new Node(0, new Coordinates(5, 4)),
                new Node(1, new Coordinates(4, 5)),
                new Node(2, new Coordinates(2, 4)),
                new Node(3, new Coordinates(1, 2)),
                new Node(4, new Coordinates(2, 1)),
                new Node(5, new Coordinates(4, 2))
            };

            var terminalNodes = new List<TerminalNode>
            {
                new TerminalNode(0, new Coordinates(7, 5), new[] { (Hexagon.Azimuth300, nodes[0]) }),
                new TerminalNode(1, new Coordinates(5, 7), new[] { (Hexagon.Azimuth000, nodes[1]) }),
                new TerminalNode(2, new Coordinates(1, 5), new[] { (Hexagon.Azimuth060, nodes[2]) }),
                new TerminalNode(3, new Coordinates(-1, 1), new[] { (Hexagon.Azimuth120, nodes[3]) }),
                new TerminalNode(4, new Coordinates(1, -1), new[] { (Hexagon.Azimuth180, nodes[4]) }),
                new TerminalNode(5, new Coordinates(5, 1), new[] { (Hexagon.Azimuth240, nodes[5]) })
            };

            nodes[0].AddLinks(new[]
            {
                (Hexagon.Azimuth240, nodes[1]),
                (Hexagon.Azimuth000, nodes[5]),
                (Hexagon.Azimuth120, terminalNodes[0])
            });

            nodes[1].AddLinks(new[]
            {
                (Hexagon.Azimuth300, nodes[2]),
                (Hexagon.Azimuth060, nodes[0]),
                (Hexagon.Azimuth180, terminalNodes[1])
            });

            nodes[2].AddLinks(new[]
            {
                (Hexagon.Azimuth240, terminalNodes[2]),
                (Hexagon.Azimuth000, nodes[3]),
                (Hexagon.Azimuth120, nodes[1])
            });

            nodes[3].AddLinks(new[]
            {
                (Hexagon.Azimuth300, terminalNodes[3]),
                (Hexagon.Azimuth060, nodes[4]),
                (Hexagon.Azimuth180, nodes[2]),
            });

            nodes[4].AddLinks(new[]
            {
                (Hexagon.Azimuth240, nodes[3]),
                (Hexagon.Azimuth000, terminalNodes[4]),
                (Hexagon.Azimuth120, nodes[5])
            });

            nodes[5].AddLinks(new[]
            {
                (Hexagon.Azimuth300, nodes[4]),
                (Hexagon.Azimuth060, terminalNodes[5]),
                (Hexagon.Azimuth180, nodes[0])
            });

            var tableau = new Tableau(shape, nodes, terminalNodes);
            var endPoints = new List<TerminalNode>
            {
                terminalNodes[1],
                terminalNodes[2],
                terminalNodes[3]
            };

            var steps = new List<Step>
            {
                new Step(nodes[0], Hexagon.Azimuth300)
            };

            var pathFinder = new PathFinder(tableau, 0, endPoints, 1);

            // Act
            var result = await pathFinder.Explore(
                steps,
                TimeSpan.Zero,
                0L,
                TimeSpan.Zero,
                CancellationToken.None);

            // Assert
            Assert.Equal(6, result.routeCount);
        }
    }
}
