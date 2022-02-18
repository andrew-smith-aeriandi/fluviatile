using System;

namespace Fluviatile.Grid
{
    public class SymmetryTransform : ISymmetryTransform
    {
        private readonly Func<Coordinates, Coordinates> _transform;

        public SymmetryTransform(
            string name,
            SymmetryType type,
            Func<Coordinates, Coordinates> transform)
        {
            Name = name;
            SymmetryType = type;
            _transform = transform;
        }

        public string Name { get; }

        public SymmetryType SymmetryType { get; }

        public Coordinates Transform(Coordinates coordinates)
        {
            return _transform(coordinates);
        }
    }
}
