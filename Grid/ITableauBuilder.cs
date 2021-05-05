namespace Fluviatile.Grid
{
    public interface ITableauFactory
    {
        Shape Shape { get; }

        int Size { get; }

        Tableau Create();
    }
}