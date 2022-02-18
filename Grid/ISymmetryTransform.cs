namespace Fluviatile.Grid
{
    public interface ISymmetryTransform
    {
        public string Name { get; }
        public SymmetryType SymmetryType { get; }
        public Coordinates Transform(Coordinates coordinates);
    }
}
