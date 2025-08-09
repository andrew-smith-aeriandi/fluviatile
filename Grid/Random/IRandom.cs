namespace Fluviatile.Grid.Random
{
    public interface IRandom
    {
        int Seed { get; }

        int Choose(int count);

        bool Try(int numerator, int denominator);

        bool Try(double probability);
    }
}
