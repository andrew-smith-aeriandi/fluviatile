namespace Solver.Framework;

public record SolverOptions
{
    public readonly static SolverOptions Default = new()
    {
        MaxHypotheticals = 0,
        MaxRuleInvocations = 10000,
        RulePermutatorOption = PermutatorOption.Identity,
        OutputSolution = true
    };

    public int MaxHypotheticals { get; init; }

    public int MaxRuleInvocations { get; init; }

    public PermutatorOption RulePermutatorOption { get; init; }

    public bool OutputSolution { get; init; }
}
