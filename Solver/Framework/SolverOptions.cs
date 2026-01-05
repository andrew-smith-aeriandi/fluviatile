namespace Solver.Framework;

public record SolverOptions
{
    public readonly static SolverOptions Default = new()
    {
        MaxHypotheticals = 0,
        MaxRuleInvocations = 10000
    };

    public int MaxHypotheticals { get; init; }

    public int MaxRuleInvocations { get; init; }
}
