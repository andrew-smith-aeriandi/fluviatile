﻿namespace Combinations;

public readonly struct Coordinate
{
    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }

    public static Coordinate operator +(Coordinate a, Coordinate b)
    {
        return new Coordinate(a.X + b.X, a.Y + b.Y);
    }

    public static Coordinate operator -(Coordinate a, Coordinate b)
    {
        return new Coordinate(a.X - b.X, a.Y - b.Y);
    }

    public override string ToString()
    {
        return $"({X},{Y})";
    }
}
