using Solver.Components;
using Solver.Rules;
using System.Linq.Expressions;

namespace Solver.Framework;

public class RulesetFactory
{
    private readonly List<Func<Tableau, IRule>> _ruleFactories;
    private readonly Func<Tableau, IRule> _housekeepingRuleFactory;

    public static RulesetFactory Create(
        Type housekeepingRule,
        params IEnumerable<Type> rules)
    {
        return new RulesetFactory(housekeepingRule, rules);
    }

    public RulesetFactory(
        Type housekeepingRule,
        params IEnumerable<Type> rules)
    {
        _housekeepingRuleFactory = CreateRuleFactory(housekeepingRule);
        _ruleFactories = [.. rules.Select(rule => CreateRuleFactory(rule))];
    }

    public Ruleset Create(Tableau tableau)
    {
        return new Ruleset(
            _ruleFactories.Select(factory => factory(tableau)),
            _housekeepingRuleFactory(tableau));
    }

    private static Func<Tableau, IRule> CreateRuleFactory(Type type)
    {
        if (!type.IsSubclassOf(typeof(Rule)))
        {
            throw new Exception($"Type {type.FullName} must derive from {typeof(Rule).FullName}.");
        }

        var constructorInfo = type.GetConstructor([typeof(Tableau)]);
        if (constructorInfo is null)
        {
            throw new Exception($"Type {type.FullName} must have a constructor that takes a single parameter of type {typeof(Tableau).FullName}");
        }

        var parameter = Expression.Parameter(typeof(Tableau));
        var constructor = Expression.New(constructorInfo, parameter);
        var lambda = Expression.Lambda<Func<Tableau, IRule>>(constructor, parameter);

        return lambda.Compile();
    }
}
