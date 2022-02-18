namespace Fluviatile.Grid
{
    public struct Coordinates
    {
        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }

        public static Coordinates operator +(Coordinates a, Coordinates b)
        {
            return new Coordinates(a.X + b.X, a.Y + b.Y);
        }

        public static Coordinates operator -(Coordinates a, Coordinates b)
        {
            return new Coordinates(a.X - b.X, a.Y - b.Y);
        }

        public override bool Equals(object obj)
        {
             return obj is Coordinates other && this == other;
        }

        public static bool operator ==(Coordinates a, Coordinates b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Coordinates a, Coordinates b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return X ^ (Y << 16);
            }
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}
