using Solver.Components;

namespace Solver.Framework;

public class SolverRunner
{
    private readonly RulesetFactory _rulesetFactory;
    private readonly SolverOptions _options;

    public SolverRunner(
        RulesetFactory rulesetFactory,
        SolverOptions options)
    {
        ArgumentNullException.ThrowIfNull(rulesetFactory);
        ArgumentNullException.ThrowIfNull(options);

        _rulesetFactory = rulesetFactory;
        _options = options;
    }

    public IEnumerable<SolverState> Solve(Tableau tableau)
    {
        ArgumentNullException.ThrowIfNull(tableau);

        var state = new SolverState(tableau, _rulesetFactory, _options);
        return Solve(state);
    }

    private List<SolverState> Solve(SolverState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var states = new List<SolverState>();
        state.Solve();

        if (state.IsComplete)
        {
            states.Add(state);
            return states;
        }

        var queue = new Queue<SolverState>();
        queue.Enqueue(state);

        while (queue.TryDequeue(out var dequeuedState))
        {
            foreach (var resultantState in SolveWithHypothetical(dequeuedState))
            {
                if (resultantState.IsComplete)
                {
                    states.Add(resultantState);
                }
                else
                {
                    queue.Enqueue(resultantState);
                }
            }
        }

        return states;
    }

    private IEnumerable<SolverState> SolveWithHypothetical(SolverState state)
    {
        foreach (var hypothetical in state.Tableau.GetHypotheticals())
        {
            var state1 = new SolverState(state, _rulesetFactory, hypothetical, Resolution.Channel);
            state1.Solve();

            var state2 = new SolverState(state, _rulesetFactory, hypothetical, Resolution.Empty);
            state2.Solve();

            switch (state1.Status, state2.Status)
            {
                case (SolverStatus.Unsolved, SolverStatus.Unsolved):
                    if (state1.IsProgress || state2.IsProgress)
                    {
                        return [state1, state2];
                    }
                    break;

                case (SolverStatus.Solved, SolverStatus.Solved):
                case (SolverStatus.Solved, SolverStatus.Unsolved):
                case (SolverStatus.Unsolved, SolverStatus.Solved):
                    return [state1, state2];

                case (SolverStatus.Error, SolverStatus.Error):
                    state.NotifyError("Hypothetical resulted in errors.");
                    return [state];

                case (SolverStatus.Solved, SolverStatus.Error):
                case (SolverStatus.Unsolved, SolverStatus.Error):
                    return [state1];

                case (SolverStatus.Error, SolverStatus.Solved):
                case (SolverStatus.Error, SolverStatus.Unsolved):
                    return [state2];
            }
        }

        state.NotifyError("All possible hypotheticals attempted.");
        return [state];
    }
}
