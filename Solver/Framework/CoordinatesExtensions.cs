namespace Solver.Framework;

public static class CoordinatesExtensions
{
    public static Coordinates Add(this Coordinates coordinates, int x, int y)
    {
        return new Coordinates(coordinates.X + x, coordinates.Y + y);
    }

    public static Coordinates Subtract(this Coordinates coordinates, int x, int y)
    {
        return new Coordinates(coordinates.X - x, coordinates.Y - y);
    }

    public static Coordinates Multiply(this Coordinates coordinates, int factor)
    {
        return new Coordinates(coordinates.X * factor, coordinates.Y * factor);
    }

    public static Coordinates Divide(this Coordinates coordinates, int factor)
    {
        if (coordinates.X % factor != 0)
        {
            throw new ArithmeticException($"{factor} does not divide x-coordinate {coordinates}");
        }

        if (coordinates.Y % factor != 0)
        {
            throw new ArithmeticException($"{factor} does not divide y-coordinate {coordinates}");
        }

        return new Coordinates(coordinates.X / factor, coordinates.Y / factor);
    }
}
