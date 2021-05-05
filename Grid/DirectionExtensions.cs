namespace Fluviatile.Grid
{
    public static class DirectionExtensions
    {
        public static Direction Turn(this Direction direction, Torsion torsion)
        {
            return new Direction((direction.Value + torsion.Value + Direction.Modulus) % Direction.Modulus);
        }
    }
}
