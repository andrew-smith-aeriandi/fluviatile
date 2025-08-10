namespace Fluviatile.Grid
{
    public readonly struct Torsion
    {
        public static readonly Torsion None = new(0);
        public static readonly Torsion Left = new(-1);
        public static readonly Torsion Right = new(1);

        public Torsion(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public override bool Equals(object obj)
        {
            return obj is Torsion torsion && torsion.Value == Value;
        }

        public static bool operator ==(Torsion left, Torsion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Torsion left, Torsion right)
        {
            return !(left == right);
        }

        public static Torsion operator +(Torsion lhs, Torsion rhs)
        {
            return new Torsion(lhs.Value + rhs.Value);
        }

        public static Torsion operator -(Torsion lhs, Torsion rhs)
        {
            return new Torsion(lhs.Value - rhs.Value);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
