using System.Text.Json;

namespace Solver.Framework;

public class RulePrioritiser : IRulePrioritiser
{
    private readonly Dictionary<RuleType, int> _rulePriorities;
    private readonly int _defaultPriority;
    private readonly Lazy<string> _description;

    public static RulePrioritiser Create(
        IList<RuleType> ruleTypes,
        IRulePrioritiser? prioritiser = null,
        Func<IList<RuleType>, IList<RuleType>>? permutator = null)
    {
        var orderedRuleTypes = prioritiser is not null
            ? [.. ruleTypes.OrderBy(prioritiser.GetPriority)]
            : ruleTypes;

        var permutedRuleTypes = permutator is not null
            ? permutator(orderedRuleTypes)
            : orderedRuleTypes;

        var priorities = permutedRuleTypes
            .Select((rule, index) => new KeyValuePair<RuleType, int>(rule, index + 1))
            .ToDictionary();

        var defaultPriority = priorities.Count + 1;

        return new RulePrioritiser(priorities, defaultPriority);
    }

    private RulePrioritiser(
        Dictionary<RuleType, int> priorities,
        int defaultPriority)
    {
        _rulePriorities = priorities;
        _defaultPriority = defaultPriority;

        _description = new Lazy<string>(() =>
            JsonSerializer.Serialize(
                _rulePriorities
                    .OrderBy(item => item.Value)
                    .Select(item => item.Key.ToString())));
    }

    public int DefaultPriority => _defaultPriority;

    public IReadOnlyDictionary<RuleType, int> RulePriorities => _rulePriorities;

    public int GetPriority(RuleType ruleType)
    {
        return _rulePriorities.TryGetValue(ruleType, out var priority)
            ? priority
            : _defaultPriority;
    }

    public override string ToString()
    {
        return _description.Value;
    }
}
