using System;

namespace Fluviatile.Grid
{
    public struct Direction
    {
        public const int Modulus = 6;

        public Direction(int value)
        {
            var n = value % Modulus;
            Value = n >= 0 ? n : Modulus + n;
        }

        public int Value { get; }

        public override bool Equals(object obj)
        {
            return obj is Direction direction && direction.Value == Value;
        }

        public static bool operator ==(Direction left, Direction right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Direction left, Direction right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static Direction operator +(Direction direction, Torsion torsion)
        {
            return new Direction((direction.Value + Modulus + torsion.Value % Modulus) % Modulus);
        }

        public static Direction operator -(Direction direction, Torsion torsion)
        {
            return new Direction((direction.Value + Modulus - torsion.Value % Modulus) % Modulus);
        }

        public static Torsion operator -(Direction lhs, Direction rhs)
        {
            return ((lhs.Value - rhs.Value + Modulus) % Modulus) switch
            {
                0 => Torsion.None,
                1 => Torsion.Right,
                5 => Torsion.Left,
                _ => throw new InvalidOperationException(),
            };
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
