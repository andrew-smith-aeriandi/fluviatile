using Solver.Components;
using Solver.Rules;

namespace Solver.Framework;

public class SolverState : INotifier
{
    private readonly Tableau _tableau;
    private readonly Ruleset _ruleset;
    private readonly SolverOptions _options;

    private readonly List<IResolvableComponent> _hypotheticalComponents;
    private readonly IResolvableComponent? _currentHypotheticalComponent;
    private readonly Resolution _currentHypotheticalResolution;

    private readonly PriorityQueue<RuleInvocation, int> _priorityQueue;
    private readonly FragmentedList<ResolutionResult> _resolutionResults;
    private int _ruleInvocationCount;

    private SolverResult _result;
    private string _resultDescription;
    private Exception? _exception;

    public SolverState(
        Tableau tableau,
        RulesetFactory rulesetFactory,
        SolverOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(tableau);
        ArgumentNullException.ThrowIfNull(rulesetFactory);

        _tableau = tableau;
        _ruleset = rulesetFactory.Create(tableau);
        _options = options ?? SolverOptions.Default;

        _resolutionResults = [];
        _ruleInvocationCount = 0;

        _currentHypotheticalResolution = Resolution.Unknown;
        _currentHypotheticalComponent = null;
        _hypotheticalComponents = [];

        _result = SolverResult.Unsolved;
        _resultDescription = string.Empty;
        _exception = null;

        _priorityQueue = new PriorityQueue<RuleInvocation, int>();
    }

    public SolverState(
        SolverState parent,
        RulesetFactory rulesetFactory,
        IResolvableComponent hypotheticalComponent,
        Resolution hypotheticalResolution,
        SolverOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(rulesetFactory);
        ArgumentNullException.ThrowIfNull(hypotheticalComponent);

        if (hypotheticalResolution == Resolution.Unknown)
        {
            throw new ArgumentException(
                $"Value must not be {Resolution.Unknown}",
                nameof(hypotheticalResolution));
        }

        var tableau = TableauFactory.Clone(parent.Tableau);

        _tableau = tableau;
        _ruleset = rulesetFactory.Create(tableau);
        _options = options ?? parent.Options;

        _resolutionResults = [.. parent.ResolutionResults];
        _ruleInvocationCount = parent.RuleInvocationCount;

        _currentHypotheticalResolution = hypotheticalResolution;
        _currentHypotheticalComponent = (IResolvableComponent)tableau.GetEquivalentComponent(hypotheticalComponent);
        _hypotheticalComponents = parent.HypotheticalComponents
            .Select(item => (IResolvableComponent)tableau.GetEquivalentComponent(item))
            .ToList();

        _result = parent.Result;
        _resultDescription = parent.ResultDescription;
        _exception = parent.Exception;

        _priorityQueue = new PriorityQueue<RuleInvocation, int>();
    }

    public SolverOptions Options => _options;

    public Tableau Tableau => _tableau;

    public FragmentedList<ResolutionResult> ResolutionResults => _resolutionResults;

    public bool IsProgress => _resolutionResults.CurrentCount > 0;

    public int RuleInvocationCount => _ruleInvocationCount;

    public IReadOnlyList<IResolvableComponent> HypotheticalComponents => _hypotheticalComponents;

    public int HypotheticalComponentsCount => _hypotheticalComponents.Count;

    public IResolvableComponent? CurrentHypotheticalComponent => _currentHypotheticalComponent;

    public Resolution CurrentHypotheticalResolution => _currentHypotheticalResolution;

    public SolverResult Result => _result;

    public bool IsComplete => _result != SolverResult.Unsolved;

    public string ResultDescription => _resultDescription;

    public Exception? Exception => _exception;

    public void NotifyError(string description)
    {
        _result = SolverResult.Error;
        _resultDescription = description;
    }

    public void NotifyResolution(
        IComponent component,
        ResolutionReason reason = ResolutionReason.Unspecified)
    {
        // Log resolution reason
        _resolutionResults.Add(new ResolutionResult(component, reason));

        // Fix up tableau and aisle counts
        Tableau.NotifyResolution(component);

        // Resolve any components that are a direct consequnce of this resolution
        _ruleset.HousekeepingRule.Invoke(component, this);

        // Enqueue new rules
        EnqueueRules(component);
    }

    public void Solve()
    {
        if (_result != SolverResult.Unsolved)
        {
            return;
        }

        try
        {
            var priorResolutionResultsCount = _resolutionResults.CurrentCount;
            var hasEnqueuedTableauRules = false;

            if (_ruleInvocationCount == 0)
            {
                // Enqueue initial rules 
                EnqueueRules(Tableau);
                hasEnqueuedTableauRules = true;
            }

            if (_currentHypotheticalComponent is not null)
            {
                // Try to resolve hypothetical
                if (_currentHypotheticalComponent.TryResolve(
                    _currentHypotheticalResolution,
                    this,
                    ResolutionReason.Hypothetical))
                {
                    _hypotheticalComponents.Add(_currentHypotheticalComponent);
                }
                else
                {
                    _result = SolverResult.Error;
                    _resultDescription = "Failed to resolve hypothetical";
                    return;
                }
            }

            while (!_tableau.IsSolved() && _ruleInvocationCount < _options.MaxRuleInvocations)
            {
                if (_priorityQueue.TryDequeue(out var item, out _))
                {
                    // Invoke rule
                    _ruleInvocationCount += 1;
                    item.Rule.Invoke(item.Component, this);
                }
                else if (_resolutionResults.Count > priorResolutionResultsCount)
                {
                    // No more queued rules but some progress has been made in solving the tableau
                    priorResolutionResultsCount = _resolutionResults.CurrentCount;

                    // Try enqueuing tableau rules
                    EnqueueRules(Tableau);
                    hasEnqueuedTableauRules = true;
                }
                else if (!hasEnqueuedTableauRules)
                {
                    // Try enqueuing tableau rules once more
                    EnqueueRules(Tableau);
                    hasEnqueuedTableauRules = true;
                }
                else
                {
                    // Break out of loop if no progress has been made
                    break;
                }
            }

            if (_tableau.IsSolved())
            {
                _result = SolverResult.Solved;
            }
            else if (_ruleInvocationCount >= _options.MaxRuleInvocations)
            {
                _result = SolverResult.Error;
                _resultDescription = $"Maximum rule invocation limit ({_options.MaxRuleInvocations}) reached.";
            }
        }
        catch (Exception ex)
        {
            _result = SolverResult.Error;
            _resultDescription = ex.Message;
            _exception = ex;
        }
    }

    private void EnqueueRules(IComponent component)
    {
        foreach (var (rule, priority) in _ruleset.GetRules(component))
        {
            _priorityQueue.Enqueue(new(rule, component), priority);
        }
    }
}
