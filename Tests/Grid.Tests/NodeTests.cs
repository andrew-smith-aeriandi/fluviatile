using Fluviatile.Grid;
using System;
using System.Collections.Generic;
using Xunit;

namespace Grid.Tests
{
    public class NodeTests
    {
        [Fact]
        public void Constructor_PopulatesProperties()
        {
            // Act
            var node = new Node(2, new Coordinates(11, 13));

            // Assert
            Assert.Equal(2, node.Index);
            Assert.Equal(11, node.Coordinates.X);
            Assert.Equal(13, node.Coordinates.Y);
            Assert.Null(node.Links);
        }

        [Fact]
        public void ToString_ReturnsExpectedString()
        {
            // Arrange
            var node = new Node(2, new Coordinates(11, 13));

            // Act
            var result = node.ToString();

            // Assert
            Assert.Equal("(11,13)", result);
        }

        [Fact]
        public void AddLinks_PopulatesLinksProperty()
        {
            // Arrange
            var node = new Node(2, new Coordinates(7, 8));

            // Act
            node.AddLinks(new List<(Direction, Node)>
            {
                (new Direction(1), new Node(3, new Coordinates(5, 7))),
                (new Direction(3), new Node(4, new Coordinates(8, 7))),
                (new Direction(5), new Node(5, new Coordinates(8, 10)))
            });

            // Assert
            Assert.Equal(new Coordinates(5, 7), node.Links[new Direction(1)].Coordinates);
            Assert.Equal(new Coordinates(8, 7), node.Links[new Direction(3)].Coordinates);
            Assert.Equal(new Coordinates(8, 10), node.Links[new Direction(5)].Coordinates);
        }

        [Fact]
        public void AddLinks_WhenLinksAlreadyPopulated_Throws()
        {
            // Arrange
            var node = new Node(2, new Coordinates(7, 8));

            node.AddLinks(new List<(Direction, Node)>
            {
                (new Direction(1), new Node(3, new Coordinates(5, 7))),
                (new Direction(3), new Node(4, new Coordinates(8, 7)))
            });

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => node.AddLinks(
                    new List<(Direction, Node)>
                    {
                        (new Direction(5), new Node(5, new Coordinates(8, 10)))
                    }));
        }

        [Fact]
        public void AddLinks_WhenLinksIsNull_Throws()
        {
            // Arrange
            var node = new Node(2, new Coordinates(7, 8));

            // Act + Assert
            var ex = Assert.Throws<ArgumentNullException>(
                () => node.AddLinks(null));

            // Assert
            Assert.Equal("links", ex.ParamName);
        }
    }
}
