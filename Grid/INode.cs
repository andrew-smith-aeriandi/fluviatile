namespace Fluviatile.Grid
{
    public interface INode<out T>
    {
        int Index { get; }

        T Value { get; }

        INode<T>[] Links { get; }
    }
}
