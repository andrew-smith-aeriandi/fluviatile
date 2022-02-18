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

        public abstract int Tiles { get; }

        public abstract Coordinates Centre { get; }

        public abstract int[][] NodeCountPermutations { get; set; }

        public abstract IEnumerable<ISymmetryTransform> SymmetryTransformations { get; }

        public abstract Func<Step, IEnumerable<byte[]>> GetEquivalentPathsDelegate(NodePair nodePair);

        public abstract Tableau CreateTableau();

        public abstract bool IsFullTurn(Step step1, Step step2);

        public abstract byte[] CountNodes(Step path);

        public abstract byte[] CountNodesAndNormalise(Step path, int[][] permutations);

        public override string ToString()
        {
            return $"{Name}({Size})";
        }
    }
}
