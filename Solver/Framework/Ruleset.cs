using Solver.Components;
using Solver.Rules;
using System.Reflection;

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
                var attribute = rule.GetType().GetCustomAttribute<RulePrioriryAttribute>();
                var priority = attribute?.Value ?? QueuePriority.Default;
                //var priority = QueuePriority.Default;

                foreach (var componentType in rule.GetPertinentComponents())
                {
                    if (!registry.TryGetValue(componentType, out var registeredActions))
                    {
                        registeredActions = [];
                        registry.Add(componentType, registeredActions);
                    }

                    registeredActions.Add((rule, priority));
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
