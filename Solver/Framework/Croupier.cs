namespace Solver.Framework;

public class Croupier : ICroupier
{
    private readonly Random _rng;
    private readonly double _variance;

    public Croupier()
    {
        _rng = new Random();
        _variance = 2.0;
    }

    public Croupier(int seed, double offset)
    {
        _rng = new Random(seed);
        _variance = offset * offset;
    }

    public IList<T> Shuffle<T>(IEnumerable<T> input)
    {
        var output = input.ToArray();
        _rng.Shuffle(output);

        return output;
    }

    public IList<T> Transpose<T>(IEnumerable<T> input, int count = 1)
    {
        var output = input.ToArray();
        var length = output.Length;

        while (count > 0)
        {
            var u = _rng.NextDouble();
            var v = _rng.NextDouble();
            var gaussianVariate = Math.Sqrt(-2.0 * _variance * Math.Log(u)) * Math.Sin(2.0 * Math.PI * v);

            var i = _rng.Next(length);
            var j = i + (int)Math.Floor(gaussianVariate + 0.5);

            if (j != i && j >= 0 && j < length)
            {
                (output[i], output[j]) = (output[j], output[i]);
                count -= 1;
            }
        }

        return output;
    }
}
