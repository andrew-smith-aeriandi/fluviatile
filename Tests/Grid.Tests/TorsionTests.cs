using Fluviatile.Grid;
using Xunit;

namespace Grid.Tests
{
    public class TorsionTests
    {
        [Fact]
        public void Constructor_PopulatesProperties()
        {
            // Act
            var torsion = new Torsion(4);

            // Assert
            Assert.Equal(4, torsion.Value);
        }

        [Fact]
        public void ToString_ReturnsExpectedString()
        {
            // Arrange
            var torsion = new Torsion(4);

            // Act
            var result = torsion.ToString();

            // Assert
            Assert.Equal("4", result);
        }

        [Fact]
        public void AddOperator_WithTorsion_ReturnsSumOfTorsions()
        {
            // Arrange
            var torsion1 = new Torsion(4);
            var torsion2 = new Torsion(5);

            // Act
            var result = torsion1 + torsion2;

            // Assert
            Assert.Equal(9, result.Value);
        }

        [Fact]
        public void SubtractOperator_WithTorsion_ReturnsNewTorsion()
        {
            // Arrange
            var torsion1 = new Torsion(4);
            var torsion2 = new Torsion(5);

            // Act
            var result = torsion1 - torsion2;

            // Assert
            Assert.Equal(-1, result.Value);
        }

        [Fact]
        public void Equals_WhenTorsionsAreEqual_ReturnsTrue()
        {
            // Arrange
            var torsion1 = new Torsion(-5);
            var torsion2 = new Torsion(-5);

            // Act
            var result = torsion1.Equals(torsion2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equals_WhenTorsionsDiffer_ReturnsFalse()
        {
            // Arrange
            var torsion1 = new Torsion(-5);
            var torsion2 = new Torsion(5);

            // Act
            var result = torsion1.Equals(torsion2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetHashCode_WithSameTorsions_ReturnsSameValue()
        {
            // Arrange
            var torsion1 = new Torsion(-5);
            var torsion2 = new Torsion(-5);

            // Act
            var hashCode1 = torsion1.GetHashCode();
            var hashCode2 = torsion2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        [Fact]
        public void GetHashCode_WithDifferentTorsions_ReturnsDifferentValue()
        {
            // Arrange
            var torsion1 = new Torsion(-5);
            var torsion2 = new Torsion(5);

            // Act
            var hashCode1 = torsion1.GetHashCode();
            var hashCode2 = torsion2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }
    }
}
