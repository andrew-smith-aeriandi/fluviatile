namespace Solver.Framework;

public static class PermutatorProvider
{
    public static Func<IList<T>, IList<T>> Get<T>(PermutatorOption option, ICroupier croupier)
    {
        return option switch
        {
            PermutatorOption.TransposeOne => GetTransposer<T>(croupier, 1),
            PermutatorOption.TransposeTwo => GetTransposer<T>(croupier, 2),
            PermutatorOption.TransposeThree => GetTransposer<T>(croupier, 3),
            PermutatorOption.Shuffle => GetShuffler<T>(croupier),
            _ => GetIdentity<T>()
        };
    }

    public static Func<IList<T>, IList<T>> GetIdentity<T>()
    {
        return items => items;
    }

    public static Func<IList<T>, IList<T>> GetShuffler<T>(ICroupier croupier)
    {
        return items => croupier.Shuffle(items);
    }

    public static Func<IList<T>, IList<T>> GetTransposer<T>(ICroupier croupier, int count)
    {
        return items => croupier.Transpose(items, count);
    }
}
