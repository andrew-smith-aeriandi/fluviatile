using Solver.Components;
using System.Diagnostics;

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
    private readonly HashSet<ResolutionReason> _resolutionReasons;
    private readonly FragmentedList<ResolutionResult> _resolutionResults;
    private int _ruleInvocationCount;
    private TimeSpan _elapsedTime;

    private SolverStatus _status;
    private string _resultDescription;
    private Exception? _exception;

    public SolverState(
        Tableau tableau,
        RulesetFactory rulesetFactory,
        IRulePrioritiser? prioritiser = null,
        SolverOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(tableau);
        ArgumentNullException.ThrowIfNull(rulesetFactory);

        _options = options ?? SolverOptions.Default;

        _tableau = tableau;
        _ruleset = rulesetFactory.Create(tableau, prioritiser);

        _resolutionReasons = [];
        _resolutionResults = [];
        _ruleInvocationCount = 0;
        _elapsedTime = TimeSpan.Zero;

        _currentHypotheticalResolution = Resolution.Unknown;
        _currentHypotheticalComponent = null;
        _hypotheticalComponents = [];

        _status = SolverStatus.Unsolved;
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

        _options = options ?? parent.Options;

        var tableau = TableauFactory.Clone(parent.Tableau);

        _tableau = tableau;
        _ruleset = rulesetFactory.Create(tableau, parent.Ruleset.Prioritiser);

        _resolutionReasons = [.. parent.ResolutionReasons];
        _resolutionResults = [.. parent.ResolutionResults];
        _ruleInvocationCount = parent.RuleInvocationCount;
        _elapsedTime = parent.ElapsedTime;

        _currentHypotheticalResolution = hypotheticalResolution;
        _currentHypotheticalComponent = (IResolvableComponent)tableau.GetEquivalentComponent(hypotheticalComponent);
        _hypotheticalComponents = parent.HypotheticalComponents
            .Select(item => (IResolvableComponent)tableau.GetEquivalentComponent(item))
            .ToList();

        _status = parent.Status;
        _resultDescription = parent.ResultDescription;
        _exception = parent.Exception;

        _priorityQueue = new PriorityQueue<RuleInvocation, int>();
    }

    /// <summary>
    /// Solver options
    /// </summary>
    public SolverOptions Options => _options;

    /// <summary>
    /// Reference to the tableau
    /// </summary>
    public Tableau Tableau => _tableau;


    /// <summary>
    /// The ruleset used to solve the tableau
    /// </summary>
    public Ruleset Ruleset => _ruleset;

    /// <summary>
    /// Distinct resolution reasons recorded for all solver phases
    /// </summary>
    public HashSet<ResolutionReason> ResolutionReasons => _resolutionReasons;

    /// <summary>
    /// Count of distinct resolution reasons for all solver phases
    /// </summary>
    public int ResolutionReasonCount => _resolutionReasons.Count;

    /// <summary>
    /// Cumulative collection of resolution results including prior solver phases
    /// </summary>
    public FragmentedList<ResolutionResult> ResolutionResults => _resolutionResults;

    /// <summary>
    /// Indicates whether progress has been made in solving the tableau in the current solver phase
    /// </summary>
    public bool IsProgress => _resolutionResults.CurrentCount > 0;

    /// <summary>
    /// Count of rules invoked in solving the tableau for all solver phases
    /// </summary>
    public int RuleInvocationCount => _ruleInvocationCount;

    /// <summary>
    /// Elapsed time in solving the tableau for all solver phases
    /// </summary>
    public TimeSpan ElapsedTime => _elapsedTime;

    /// <summary>
    /// Ordered list of components that were resolved as hypotheticals for all solver phases 
    /// </summary>
    public IReadOnlyList<IResolvableComponent> HypotheticalComponents => _hypotheticalComponents;

    /// <summary>
    /// Count of components that were resolved as hypotheticals for all solver phases
    /// </summary>
    public int HypotheticalComponentsCount => _hypotheticalComponents.Count;

    /// <summary>
    /// Reference to the component (if any) that was resolved as a hypothetical in the current solver phase
    /// </summary>
    public IResolvableComponent? CurrentHypotheticalComponent => _currentHypotheticalComponent;

    /// <summary>
    /// Resolution state of the component (if any) that was resolved as a hypothetical in the current solver phase
    /// </summary>
    public Resolution CurrentHypotheticalResolution => _currentHypotheticalResolution;

    /// <summary>
    /// Solver status
    /// </summary>
    public SolverStatus Status => _status;

    /// <summary>
    /// Indicates whether the solver has completed (either Solved or Error)
    /// </summary>
    public bool IsComplete => _status != SolverStatus.Unsolved;

    /// <summary>
    /// Textual description of the solver result
    /// </summary>
    public string ResultDescription => _resultDescription;

    /// <summary>
    /// Exception that was thrown (if any) for a solver that has completed with an Error status
    /// </summary>
    public Exception? Exception => _exception;

    /// <summary>
    /// Sets the solver status as Error and specifies a human-readable description of the error
    /// </summary>
    public void NotifyError(string description)
    {
        _resultDescription = description;
        _status = SolverStatus.Error;
    }

    /// <summary>
    /// Sets the solver status as Error, using the specified exception message as the description of the error
    /// </summary>
    public void NotifyError(Exception exception)
    {
        _exception = exception;
        _resultDescription = exception.Message;
        _status = SolverStatus.Error;
    }

    /// <summary>
    /// Updates the solver state whenever a component is resolved 
    /// </summary>
    /// <remarks>
    /// This method should be called whenever a component is resolved so that the following can occur:
    /// <list type="bullet">
    /// <item>The resolution reason and result are logged</item>
    /// <item>Tableau and aisle counts are updated</item>
    /// <item>Any housekeeping rules are invoked immediately</item>
    /// <item>New resolution rules are enqueued</item>
    /// </list>
    /// </remarks>
    /// <param name="component"></param>
    /// <param name="reason"></param>
    public void NotifyResolution(
        IComponent component,
        ResolutionReason reason = ResolutionReason.Unspecified)
    {
        // Log resolution reason
        _resolutionReasons.Add(reason);
        _resolutionResults.Add(new ResolutionResult(component, reason));

        // Fix up tableau and aisle counts
        Tableau.NotifyResolution(component);

        // Resolve any components that are a direct consequnce of this resolution
        _ruleset.HousekeepingRule.Invoke(component, this);

        // Enqueue new rules
        EnqueueRules(component);
    }

    /// <summary>
    /// Invoke the solver
    /// </summary>
    public void Solve()
    {
        if (_status != SolverStatus.Unsolved)
        {
            return;
        }

        var timestamp = Stopwatch.GetTimestamp();

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
                    NotifyError("Failed to resolve hypothetical");
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
                _status = SolverStatus.Solved;
            }
            else if (_ruleInvocationCount >= _options.MaxRuleInvocations)
            {
                NotifyError($"Maximum rule invocation limit ({_options.MaxRuleInvocations}) reached.");
            }
        }
        catch (Exception ex)
        {
            NotifyError(ex);
        }
        finally
        {
            _elapsedTime += Stopwatch.GetElapsedTime(timestamp);
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
