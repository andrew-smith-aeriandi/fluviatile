using Solver.Components;
using Solver.Rules;

namespace Solver.Framework;

public class SolverState : INotifier
{
    private readonly Tableau _tableau;
    private readonly List<ResolutionResult> _resolutionResults;
    private readonly Dictionary<Type, List<(IRule, int)>> _ruleRegistry;
    private readonly PriorityQueue<RuleInvocation, int> _priorityQueue;
    private readonly IRule _housekeepingRule;

    public SolverState(
        Tableau tableau,
        IEnumerable<IRule> rules,
        IRule housekeepingRule)
    {
        _tableau = tableau;

        _housekeepingRule = housekeepingRule;
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

        _priorityQueue = new PriorityQueue<RuleInvocation, int>();
        _resolutionResults = [];
    }

    public Tableau Tableau => _tableau;

    public IReadOnlyList<ResolutionResult> ResolutionResults => _resolutionResults;

    private void EnqueueRules(IComponent component)
    {
        if (_ruleRegistry.TryGetValue(component.GetType(), out var rules))
        {
            foreach (var (rule, priority) in rules)
            {
                _priorityQueue.Enqueue(new(rule, component), priority);
            }
        }
    }

    public void NotifyResolution(IComponent component, ResolutionReason reason = ResolutionReason.Unspecified)
    {
        // Log resolution reason
        _resolutionResults.Add(new ResolutionResult(component, reason));

        // Fix up tableau and aisle counts
        Tableau.NotifyResolution(component);

        // Resolve any components that are a direct consequnce of this resolution
        _housekeepingRule.Invoke(component, this);

        // Enqueue new rules
        EnqueueRules(component);
    }

    public bool Solve(int maxRuleInvocations, out int ruleInvocations)
    {
        ruleInvocations = 0;

        while (!Tableau.IsSolved() && ruleInvocations < maxRuleInvocations)
        {
            if (_priorityQueue.TryDequeue(out var item, out _))
            {
                ruleInvocations += 1;
                item.Rule.Invoke(item.Component, this);
            }
            else if (_priorityQueue.Count == 0)
            {
                EnqueueRules(Tableau);
                EnqueueRules(Tableau.Thalweg);
            }
        }

        return Tableau.IsSolved();
    }
}
