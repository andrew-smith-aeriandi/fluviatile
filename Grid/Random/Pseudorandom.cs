namespace Fluviatile.Grid.Random
{
    public class Pseudorandom : IRandom
    {
        private readonly System.Random _random;

        public Pseudorandom(int seed)
        {
            Seed = seed;
            _random = new System.Random(seed);
        }

        public int Seed { get; }

        public int Choose(int count)
        {
            return _random.Next(count);
        }

        public bool Try(int numerator, int denominator)
        {
            return _random.Next(denominator) < numerator;
        }

        public bool Try(double probability)
        {
            return _random.NextDouble() < probability;
        }
    }
}
