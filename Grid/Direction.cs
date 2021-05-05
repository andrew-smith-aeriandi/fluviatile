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

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
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
            switch ((lhs.Value - rhs.Value + Modulus) % Modulus)
            {
                case 0: return Torsion.None;
                case 1: return Torsion.Right;
                case 5: return Torsion.Left;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
