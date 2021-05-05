namespace WindowsFormsApp2
{
    public interface IRandom
    {
        int Seed { get; }

        int Choose(int count);
        bool Try(double probability);
    }
}
