using Solver.Components;
using Solver.Rules;

namespace Solver.Framework;

public record Ruleset
{
    public required IRule HousekeepingRule { get; init; }

    public required IReadOnlyDictionary<ComponentType, IList<(IRule Rule, int Priority)>> RuleRegistry { get; init; }

    public required IRulePrioritiser Prioritiser { get; init; }

    public IList<(IRule Rule, int Priority)> GetRules(ComponentType componentType)
    {
        return RuleRegistry.TryGetValue(componentType, out var rules)
            ? rules
            : [];
    }

    public IList<(IRule Rule, int Priority)> GetRules(IComponent component)
    {
        return GetRules(component.GetComponentType());
    }
}
