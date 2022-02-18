namespace Canvas
{
    public interface IRandom
    {
        int Seed { get; }

        int Choose(int count);

        bool Try(double probability);
    }
}
