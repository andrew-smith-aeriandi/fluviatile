namespace Fluviatile.Grid
{
    public readonly struct Aisle
    {
        public Aisle(int direction, int index)
        {
            Direction = direction;
            Index = index;
        }

        public int Direction { get; }

        public int Index { get; }

        public override string ToString()
        {
            return $"{Direction}:{Index}";
        }
    }
}
