using Solver.Components;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Solver.Framework.LinqExtensions;

namespace Solver.Framework;

public record Grid
{
    public const int MinSize = 1;
    public const int MaxSize = 8;

    public const int TileVertexCount = 3;

    public const int AxisCount = 3;
    public const int AxisMask = (int)(Axis.X | Axis.Y | Axis.Z);

    public const int Scale = 3;
    public const int OneThird = 1;
    public const int TwoThird = 2;

    public const int ExitCount = 2;

    private readonly int[] _aisleTileCounts;

    public readonly static Shape ShapeUp = new()
    {
        Orientation = Orientation.Up,
        VertexOffsets =
        [
            new Coordinates(-TwoThird, OneThird, OneThird),
            new Coordinates(OneThird, -TwoThird, OneThird),
            new Coordinates(OneThird, OneThird, -TwoThird)
        ]
    };

    public readonly static Shape ShapeDown = new()
    {
        Orientation = Orientation.Down,
        VertexOffsets =
        [
            new Coordinates(TwoThird, -OneThird, -OneThird),
            new Coordinates(-OneThird, TwoThird, -OneThird),
            new Coordinates(-OneThird, -OneThird, TwoThird)
        ]
    };

    public Grid(int size)
    {
        if (size < MinSize || size > MaxSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                $"Value must be between {MinSize} and {MaxSize}.");
        }

        Size = size;
        Radius = size * Scale;
        AisleCountPerAxis = size * 2;
        AisleCount = AisleCountPerAxis * AxisCount;
        TileCount = 6 * Size * Size;
        VertexCount = 3 * Size * (Size + 1) + 1;
        EdgeCount = TileCount + VertexCount - 1;

        var vertices = new List<Coordinates>();
        for (var x = -size; x <= size; x++)
        {
            for (var y = -size; y <= size; y++)
            {
                if (Math.Abs(x + y) <= size)
                {
                    vertices.Add(new Coordinates(x * Scale, y * Scale));
                }
            }
        }

        Vertices = vertices.ToFrozenSet();

        _aisleTileCounts = Enumerable.Range(0, AisleCountPerAxis)
            .Select(index => index < Size
                ? 2 * (index + Size) + 1
                : 2 * (3 * Size - index) - 1)
            .ToArray();
    }

    public int Size { get; }

    public int TileCount { get; }

    public int VertexCount { get; }

    public int EdgeCount { get; }

    public int AisleCount { get; }

    public int AisleCountPerAxis { get; }

    public int Radius { get; }

    public FrozenSet<Coordinates> Vertices { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CoordinateLength(Index index)
    {
        return Scale * index.GetOffset(Size);
    }

    public int AisleTileCount(int index)
    {
        return 0 <= index && index < AisleCountPerAxis
            ? _aisleTileCounts[index]
            : throw new ArgumentOutOfRangeException(nameof(index), $"Value mus be between 0 and {AisleCountPerAxis - 1}");
    }

    public IEnumerable<(Axis, int)> GetAisleKeys(Tile tile)
    {
        yield return (Axis.X, GetAisleIndex(tile.Coordinates.X));
        yield return (Axis.Y, GetAisleIndex(tile.Coordinates.Y));
        yield return (Axis.Z, GetAisleIndex(tile.Coordinates.Z));
    }

    public bool TryGetAisleIndex(int coordinate, out int index)
    {
        index = GetAisleIndex(coordinate);
        return index >= 0 && index < AisleCountPerAxis;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAisleIndex(int coordinate)
    {
        return (coordinate >= 0 ? coordinate : coordinate - TwoThird) / Scale + Size;
    }

    public Orientation GetTileOrientation(Coordinates centre)
    {
        return Maths.Mod(centre.X, Scale) switch
        {
            OneThird => Orientation.Down,
            TwoThird => Orientation.Up,
            _ => throw new ArgumentException($"Invalid coordinates: {centre}", nameof(centre))
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InRange(int coordinate)
    {
        return Math.Abs(coordinate) <= Radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InRange(Coordinates coordinates)
    {
        return InRange(coordinates.X) && InRange(coordinates.Y) && InRange(coordinates.Z);
    }

    public bool TryGetAdjacentCoordinates(
        Tile? tile,
        Axis direction,
        out Coordinates centre)
    {
        if (tile is null)
        {
            centre = default;
            return false;
        }

        var vector =  (tile.Orientation, direction) switch
        {
            (Orientation.Up, Axis.X) => new Coordinates(TwoThird, -OneThird, -OneThird),
            (Orientation.Down, Axis.X) => new Coordinates(-TwoThird, OneThird, OneThird),
            (Orientation.Up, Axis.Y) => new Coordinates(-OneThird, TwoThird, -OneThird),
            (Orientation.Down, Axis.Y) => new Coordinates(OneThird, -TwoThird, OneThird),
            (Orientation.Up, Axis.Z) => new Coordinates(-OneThird, -OneThird, TwoThird),
            (Orientation.Down, Axis.Z) => new Coordinates(OneThird, OneThird, -TwoThird),
            _ => throw new UnreachableException($"Invalid (orientaion, direction) combination: ({tile.Orientation}, {direction})")
        };

        centre = tile.Coordinates + vector;
        return true;
    }

    public bool TryGetTileCentreCoordinates(
        Orientation orientation,
        int aisleXIndex,
        int aisleYIndex,
        out Coordinates centre)
    {
        switch (orientation)
        {
            case Orientation.Down:
                centre = new Coordinates(
                    (aisleXIndex - Size) * Scale + OneThird,
                    (aisleYIndex - Size) * Scale + OneThird);

                return InRange(centre);

            case Orientation.Up:
                centre = new Coordinates(
                    (aisleXIndex - Size) * Scale + TwoThird,
                    (aisleYIndex - Size) * Scale + TwoThird);

                return InRange(centre);
        }

        centre = default;
        return false;
    }

    public IEnumerable<Edge> CreateEdgesFromVertices(IEnumerable<Coordinates> vertices)
    {
        return vertices.SelectWithNext(
            (first, second) => new Edge(first, second, this),
            SelectWithNextOption.LoopBackToStart);
    }

    public static bool IncludesAllAxes(IEnumerable<Axis> axes)
    {
        var count = 0;
        var mask = 0;

        foreach (var axis in axes)
        {
            count += 1;
            mask |= (int)axis;
        }

        return count == AxisCount && mask == (1 << AxisCount) - 1;
    }

    public static IEnumerable<Axis> Axes
    {
        get
        {
            yield return Axis.X;
            yield return Axis.Y;
            yield return Axis.Z;
        }
    }

    public override string ToString()
    {
        return $"{nameof(Grid)}, Size:{Size}, Scale:{Scale}";
    }
}
