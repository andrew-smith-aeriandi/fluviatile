namespace Solver.Framework;

public readonly struct UnorderedPair<T>(T item1, T item2)
    : IEquatable<UnorderedPair<T>>
{
    private readonly T _v0 = item1 ?? throw new ArgumentNullException(nameof(item1));
    private readonly T _v1 = item2 ?? throw new ArgumentNullException(nameof(item2));

    public IEnumerable<T> Values
    {
        get
        {
            yield return _v0;
            yield return _v1;
        }
    }

    public T this[int index] => index switch
    {
        0 => _v0,
        1 => _v1,
        _ => throw new IndexOutOfRangeException("Index must be 0 or 1.")
    };

    public void Deconstruct(out T item1, out T item2)
    {
        item1 = _v0;
        item2 = _v1;
    }

    public bool EqualsEither(T value)
    {
        return _v0!.Equals(value) || _v1!.Equals(value);
    }

    public bool Equals(UnorderedPair<T> other)
    {
        return _v0!.Equals(other[0]) && _v1!.Equals(other[1]) ||
               _v0!.Equals(other[1]) && _v1!.Equals(other[0]);
    }

    public override bool Equals(object? obj)
    {
        return obj is UnorderedPair<T> pair && pair.Equals(this);
    }

    public override int GetHashCode()
    {
        var hashX = _v0?.GetHashCode() ?? 0;
        var hashY = _v1?.GetHashCode() ?? 0;

        unchecked
        {
            return hashX > hashY
                ? hashY * 127 + hashX
                : hashX * 127 + hashY;
        }
    }

    public static bool operator ==(UnorderedPair<T> left, UnorderedPair<T> right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(UnorderedPair<T> left, UnorderedPair<T> right)
    {
        return !Equals(left, right);
    }

    public static implicit operator UnorderedPair<T>((T, T) pair)
    {
        return new(pair.Item1, pair.Item2);
    }

    public override string ToString()
    {
        return $"{{{_v0},{_v1}}}";
    }
}
