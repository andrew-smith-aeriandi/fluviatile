using Solver.Components;
using Solver.Rules;

namespace Solver.Framework;

public class Ruleset
{
    private readonly Dictionary<Type, List<(IRule, int)>> _ruleRegistry;
    private readonly IRule _housekeepingRule;

    public Ruleset(
        IEnumerable<IRule> rules,
        IRule housekeepingRule)
    {
        _ruleRegistry = rules.Aggregate(
            new Dictionary<Type, List<(IRule, int)>>(),
            (registry, rule) =>
            {
                foreach (var componentType in rule.GetPertinentComponents())
                {
                    if (!registry.TryGetValue(componentType, out var registeredActions))
                    {
                        registeredActions = [];
                        registry.Add(componentType, registeredActions);
                    }

                    registeredActions.Add((rule, QueuePriority.Default));
                }

                return registry;
            });

        _housekeepingRule = housekeepingRule;
    }

    public IRule HousekeepingRule => _housekeepingRule;

    public IEnumerable<(IRule Rule, int Priority)> GetRules(IComponent component)
    {
        return _ruleRegistry.TryGetValue(component.GetType(), out var rules)
            ? rules
            : [];
    }
}
