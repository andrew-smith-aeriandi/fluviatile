using System;
using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public abstract class Shape
    {
        protected Shape(string name, int edges, int size, int scale)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            Name = name;
            Edges = edges;
            Size = size;
            Scale = scale;
        }

        public string Name { get; }

        public int Edges { get; }

        public int Size { get; }

        public int Scale { get; }

        public abstract Coordinates Centre { get; }

        public abstract IEnumerable<Func<Coordinates, Coordinates>> SymmetryTransformations();

        public abstract Tableau CreateTableau();

        public abstract bool IsFullTurn(Step step1, Step step2);

        public abstract int[] CountNodes(Step path);

        public override string ToString()
        {
            return $"{Name}({Size})";
        }
    }
}
