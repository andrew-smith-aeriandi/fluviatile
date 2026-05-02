using Solver.Components;
using Solver.Rules;
using System.Linq.Expressions;

namespace Solver.Framework;

public class RulesetFactory
{
    private readonly RuleType _housekeepingRuleType;
    private readonly Func<Tableau, IRule> _housekeepingRuleFactory;

    private readonly List<RuleType> _ruleTypes;
    private readonly Dictionary<RuleType, Func<Tableau, IRule>> _ruleFactories;

    private readonly IRulePrioritiser _defaultRulePrioritiser;

    public RulesetFactory(
        RuleType housekeepingRuleType,
        params IEnumerable<RuleType> ruleTypes)
    {
        _housekeepingRuleType = housekeepingRuleType;
        _housekeepingRuleFactory = CreateRuleFactory(_housekeepingRuleType);

        _ruleTypes = [.. ruleTypes];
        _ruleFactories = ruleTypes.ToDictionary(ruleType => ruleType, CreateRuleFactory);

        _defaultRulePrioritiser = RulePrioritiser.Create(_ruleTypes);
    }

    public RuleType HousekeepingRuleType => _housekeepingRuleType;

    public IReadOnlyList<RuleType> RuleTypes => _ruleTypes;

    public IRulePrioritiser DefaultRulePrioritiser => _defaultRulePrioritiser;

    public IRule CreateHousekeepingRule(Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        return _housekeepingRuleFactory(tableau);
    }

    public IRule CreateRule(
        Tableau tableau,
        RuleType ruleType)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        if (ruleType == _housekeepingRuleType)
        {
            return _housekeepingRuleFactory(tableau);
        }

        if (_ruleFactories.TryGetValue(ruleType, out var factory))
        {
            return factory(tableau);
        }

        throw new ArgumentException(
            $"Failed to get factory for {ruleType}.",
            nameof(ruleType));
    }

    public Ruleset Create(
        Tableau tableau,
        IRulePrioritiser? rulePrioritiser = null)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        rulePrioritiser ??= _defaultRulePrioritiser;

        var housekeepingRule = CreateHousekeepingRule(tableau);

        var ruleRegistry = _ruleTypes.Aggregate(
            new Dictionary<ComponentType, IList<(IRule, int)>>(),
            (registry, ruleType) =>
            {
                var rule = CreateRule(tableau, ruleType);
                var priority = rulePrioritiser.GetPriority(ruleType);

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

        return new Ruleset
        {
            HousekeepingRule = housekeepingRule,
            RuleRegistry = ruleRegistry,
            Prioritiser = rulePrioritiser
        };
    }

    private static Func<Tableau, IRule> CreateRuleFactory(RuleType ruleType)
    {
        var type = ruleType.Type;

        if (type is null)
        {
            throw new ArgumentException(
                "Type cannot be null.",
                nameof(ruleType));
        }

        if (!type.IsSubclassOf(typeof(Rule)))
        {
            throw new ArgumentException(
                $"Type {type.FullName} must derive from {typeof(Rule).FullName}.",
                nameof(ruleType));
        }

        var constructorInfo = type.GetConstructor([typeof(Tableau)]);
        if (constructorInfo is null)
        {
            throw new ArgumentException(
                $"Type {type.FullName} must have a constructor that takes a single parameter of type {typeof(Tableau).FullName}",
                nameof(ruleType));
        }

        var parameter = Expression.Parameter(typeof(Tableau));
        var constructor = Expression.New(constructorInfo, parameter);
        var lambda = Expression.Lambda<Func<Tableau, IRule>>(constructor, parameter);

        return lambda.Compile();
    }
}
