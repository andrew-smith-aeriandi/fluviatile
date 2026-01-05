using System.Collections;

namespace Solver.Framework;

public class FragmentedList<T> : IReadOnlyList<T>
{
    private readonly List<List<T>> _fragments;
    private readonly List<T> _current;
    private readonly int _priorsCount;

    public FragmentedList()
    {
        _priorsCount = 0;
        _current = [];
        _fragments = [_current];
    }

    public FragmentedList(FragmentedList<T> parent)
    {
        _priorsCount = parent.Count;
        _current = [];
        _fragments = [.. parent.Fragments, _current];
    }

    protected internal IEnumerable<List<T>> Fragments => _fragments;

    private (int, int) FindIndex(int index)
    {
        var i = 0;
        var j = index;

        while (i < _fragments.Count && j >= _fragments[i].Count)
        {
            i += 1;
            j -= _fragments.Count;
        }

        return (i, j);
    }

    public T this[int index]
    {
        get
        {
            var (i, j) = FindIndex(index);
            return _fragments[i][j];
        }
        set
        {
            var (i, j) = FindIndex(index);
            _fragments[i][j] = value;
        }
    }

    public int Count => _priorsCount + _current.Count;

    public int PriorsCount => _priorsCount;

    public int CurrentCount => _current.Count;

    public void Add(T item)
    {
        _current.Add(item);
    }

    public void Clear()
    {
        _current.Clear();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _fragments.SelectMany(item => item).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(T item)
    {
        var n = 0;
        foreach (var fragment in _fragments)
        {
            var i = fragment.IndexOf(item);
            if (i >= 0)
            {
                return n + i;
            }

            n += fragment.Count;
        }

        return -1;
    }
}
