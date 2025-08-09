namespace Solver.Framework;

public readonly struct Coordinates : IEquatable<Coordinates>
{
    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
        Z = -(x + y);
    }

    public Coordinates(int x, int y, int z)
    {
        if (x + y + z != 0)
        {
            throw new ArgumentException($"The sum of the coordinates must sum to zero: ({x},{y},{z})");
        }

        X = x;
        Y = y;
        Z = z;
    }

    public int X { get; }

    public int Y { get; }

    public int Z { get; }

    public static Coordinates operator +(Coordinates a, Coordinates b)
    {
        return new Coordinates(a.X + b.X, a.Y + b.Y);
    }

    public static Coordinates operator -(Coordinates a, Coordinates b)
    {
        return new Coordinates(a.X - b.X, a.Y - b.Y);
    }

    public static bool operator ==(Coordinates a, Coordinates b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Coordinates a, Coordinates b)
    {
        return a.X != b.X || a.Y != b.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Coordinates other && Equals(other);
    }

    public bool Equals(Coordinates other)
    {
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        return (ushort)Y << 16 | (ushort)X;
    }

    public override readonly string ToString()
    {
        return $"({X},{Y},{Z})";
    }
}
