using System;

namespace WindowsFormsApp2
{
    public class Pseudorandom : IRandom
    {
        private readonly Random _random;
        public Pseudorandom(int seed)
        {
            Seed = seed;
            _random = new Random(seed);
        }

        public int Seed { get; }

        public int Choose(int count)
        {
            return _random.Next(0, count);
        }

        public bool Try(double probability)
        {
            return _random.NextDouble() < probability;
        }
    }
}
