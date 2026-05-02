using Fluviatile.Grid;
using Fluviatile.Grid.Random;
using GridWriter;
using GridWriter.Settings;
using Solver.Components;
using Solver.Framework;
using Solver.Rules;
using System.Diagnostics;
using Tableau = Solver.Components.Tableau;

namespace Solver;

internal partial class Program
{
    private const int Size = 3;
    private readonly static SolverGrid SolverGrid = new(Size);

    private readonly static Croupier Croupier = new();

    private readonly static RuleType HousekeepingRule = RuleType.Create <HousekeepingRule>();
    private readonly static RuleType[] Rules =
    [
        RuleType.Create<AisleCountRule>(),
        RuleType.Create<MeanderRule>(),
        RuleType.Create<AisleResolutionPatternRule>(),
        RuleType.Create<TileEdgeRule>(),
        RuleType.Create<ChannelContinuityRule>(),
        RuleType.Create<AisleCountIntersectionRule>(),
        RuleType.Create<ExitCountRule>(),
        RuleType.Create<TarjansRule>()
    ];

    internal async static Task Main(string[] args)
    {
        var options = new SolverOptions
        {
            MaxHypotheticals = args.GetInteger("--max-hypotheticals", SolverOptions.Default.MaxHypotheticals),
            MaxRuleInvocations = args.GetInteger("--max-rules", SolverOptions.Default.MaxRuleInvocations),
            OutputSolution = true,
            RulePermutatorOption = PermutatorOption.Identity
        };

        var optimise = args.GetFlag("--optimise");
        var useGallery = args.GetFlag("--gallery");
        if (useGallery)
        {
            var galleryString = args.GetString("--gallery", "all");
            var gallery = galleryString;
            var index = Index.FromEnd(1);
            var useIndex = false;

            var separator = gallery.IndexOf('#');
            if (separator >= 0)
            {
                gallery = galleryString[..separator];

                var indexString = galleryString[(separator + 1)..];
                if (indexString.StartsWith('^') || indexString.StartsWith('-'))
                {
                    if (int.TryParse(indexString.AsSpan(1), out var value))
                    {
                        index = Index.FromEnd(value);
                        useIndex = true;
                    }
                }
                else
                {
                    if (int.TryParse(indexString, out var value))
                    {
                        index = Index.FromStart(value);
                        useIndex = true;
                    }
                }
            }

            var puzzles = gallery switch
            {
                "solved" => Gallery.GetAllSolved(),
                "unsolved" => Gallery.GetAllUnsolved(),
                _ => Gallery.GetAll()
            };

            if (useIndex)
            {
                var puzzle = puzzles.ElementAt(index);
                Solve(puzzle, options);
            }
            else if (optimise)
            {
                Optimise(puzzles);
            }
            else
            {
                Solve(puzzles, options);
            }
        }
        else
        {
            var random = new Pseudorandom(Environment.TickCount);
            var shape = new Hexagon(Size);
            var routeFinder = new RouteFinder(random, shape);

            await routeFinder.Initiate(Configuration.NodeCountsFilename(shape));

            var nodeCounts = NodeCountHelper.MapNodeCountsForSolver(routeFinder.SelectRandomNodeCount());
            var puzzle = new Puzzle(nodeCounts, SolverStatus.Unsolved, "Random");

            Solve(puzzle, options);
        }
    }

    private static SolverResults Solve(Puzzle puzzle, SolverOptions options)
    {
        var rulesetFactory = new RulesetFactory(HousekeepingRule, Rules);

        var rulePrioritiser = RulePrioritiser.Create(
            ruleTypes: Rules,
            permutator: PermutatorProvider.Get<RuleType>(options.RulePermutatorOption, Croupier));

        var runner = new SolverRunner(
            rulesetFactory,
            rulePrioritiser,
            options);

        var tableau = TableauFactory.Create(
            grid: SolverGrid,
            counts: puzzle.ChannelCounts);

        var results = Solve(
            runner: runner,
            tableau: tableau,
            outputState: true,
            generateSvg: true);

        return results;
    }

    public static void Optimise(IEnumerable<Puzzle> puzzles)
    {
        const int Population = 8;
        const int Generations = 3;

        var rulesetFactory = new RulesetFactory(HousekeepingRule, Rules);

        var permutator = PermutatorProvider.Get<RuleType>(PermutatorOption.Shuffle, Croupier);
        var rulePrioritisers = Enumerable.Range(0, Population)
            .Select(_ => RulePrioritiser.Create(
                ruleTypes: Rules,
                permutator: permutator))
            .ToList();

        var options = new SolverOptions
        {
            MaxRuleInvocations = 10000,
            MaxHypotheticals = 20,
            OutputSolution = false
        };

        for (var generation = 0; generation < Generations; generation++)
        {
            GC.Collect();

            var results = new List<(RulePrioritiser Prioritiser, SolverCounts SolverCounts)>();

            foreach (var rulePrioritiser in rulePrioritisers)
            {
                var runner = new SolverRunner(
                    rulesetFactory,
                    rulePrioritiser,
                    options);

                var solverCounts = new SolverCounts();

                Console.Write("Solving: [");
                foreach (var puzzle in puzzles)
                {
                    var tableau = TableauFactory.Create(
                        grid: SolverGrid,
                        counts: puzzle.ChannelCounts);

                    var solution = Solve(
                        runner: runner,
                        tableau: tableau,
                        outputState: false,
                        generateSvg: false);

                    solverCounts.NotifyStatus(solution.Status, solution.Duration);
                    Console.Write(solution.Status == SolverStatus.Solved ? '.' : 'x');
                }
                Console.WriteLine($"] {solverCounts.SolvedMeanElapsedTime.TotalMilliseconds:0.000}ms");

                results.Add((rulePrioritiser, solverCounts));
            }

            var sortedResults = results
                .OrderBy(item => item.SolverCounts.SolvedMeanElapsedTime)
                .ToList();

            foreach (var (prioritiser, solverCounts) in sortedResults)
            {
                Console.WriteLine(prioritiser.ToString());
                Console.WriteLine(solverCounts.ToString());
            }

            rulePrioritisers = sortedResults
                .Select((item, index) =>
                    RulePrioritiser.Create(
                        Rules,
                        item.Prioritiser,
                        PermutatorProvider.Get<RuleType>(
                            ((double)index / Population) switch
                            {
                                < 0.2 => PermutatorOption.Identity,
                                < 0.3 => PermutatorOption.TransposeOne,
                                < 0.4 => PermutatorOption.TransposeTwo,
                                < 0.5 => PermutatorOption.TransposeThree,
                                _ => PermutatorOption.Shuffle
                            },
                            Croupier)))
                .ToList();
        }
    }

    private static void Solve(IEnumerable<Puzzle> puzzles, SolverOptions options)
    {
        var rulesetFactory = new RulesetFactory(HousekeepingRule, Rules);

        var rulePrioritiser = RulePrioritiser.Create(
            ruleTypes: Rules,
            permutator: PermutatorProvider.Get<RuleType>(options.RulePermutatorOption, Croupier));

        var runner = new SolverRunner(
            rulesetFactory,
            rulePrioritiser,
            options);

        var solverCounts = new SolverCounts();
        var index = 0;

        Console.WriteLine(rulePrioritiser.ToString());

        if (!options.OutputSolution)
        {
            Console.Write("Solving: [");
        }

        foreach (var puzzle in puzzles)
        {
            var tableau = TableauFactory.Create(
                grid: SolverGrid,
                counts: puzzle.ChannelCounts,
                tag: $"tableau-{index}");

            var results = Solve(
                runner: runner,
                tableau: tableau,
                outputState: false,
                generateSvg: false);

            if (options.OutputSolution)
            {
                if (results.TryGetUniqueSolution(out var uniqueResult))
                {
                    Console.WriteLine($"{index}: {tableau} => {uniqueResult}, Duration: {results.Duration.TotalMilliseconds:0.000}ms");
                }
                else
                {
                    Console.WriteLine($"{index}: {tableau} => {results}, Duration: {results.Duration.TotalMilliseconds:0.000}ms");
                }
            }
            else
            {
                Console.Write(results.Status == SolverStatus.Solved ? '.' : 'x');
            }

            solverCounts.NotifyStatus(results.Status, results.Duration);
            index += 1;
        }

        Console.WriteLine("]");
        Console.WriteLine(solverCounts.ToString());
    }

    private static SolverResults Solve(
        SolverRunner runner,
        Tableau tableau,
        bool outputState = false,
        bool generateSvg = false)
    {
        var timestamp = Stopwatch.GetTimestamp();
        var states = runner.Solve(tableau).ToList();
        var elapsedTime = Stopwatch.GetElapsedTime(timestamp);

        if (outputState)
        {
            foreach (var state in states)
            {
                foreach (var aisle in state.Tableau.GetAisles()
                    .OrderBy(a => a.Axis)
                    .ThenBy(a => a.Index))
                {
                    Console.WriteLine(aisle.ToString());
                }
                Console.WriteLine();

                foreach (var tile in state.Tableau.GetTiles()
                    .OrderBy(t => t.AisleX.Index)
                    .ThenBy(t => t.AisleY.Index)
                    .ThenByDescending(t => t.AisleZ.Index))
                {
                    Console.WriteLine(tile.ToString());
                }
                Console.WriteLine();

                Console.WriteLine(state.Tableau.Thalweg.ToString());
                Console.WriteLine();

                foreach (var (component, reason) in state.ResolutionResults)
                {
                    Console.WriteLine($"{component} ({reason})");
                }
                Console.WriteLine();

                var reasons = state.ResolutionResults
                    .GroupBy(result => result.Reason)
                    .OrderBy(group => (int)group.Key)
                    .Select(group => (Reason: group.Key, Count: group.Count()))
                    .ToArray();

                foreach (var (reason, count) in reasons)
                {
                    Console.WriteLine($"{reason}: {count}");
                }
                Console.WriteLine();

                if (state.Status == SolverStatus.Error)
                {
                    Console.WriteLine($"[{string.Join(", ", state.Tableau.ChannelTileCounts.Select(n => n.ToString()))}]");

                    var foregroundColour = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (state.Exception is Exception ex)
                    {
                        Console.WriteLine($"{ex.GetType().Name}: {ex}");
                    }
                    Console.ForegroundColor = foregroundColour;
                    Console.WriteLine();
                }

                var executionResult = state.ToSolverExecutionResult();

                Console.WriteLine(executionResult.ToString());
                Console.WriteLine();
            }
        }

        var solverResults = GetSolverResults(states, elapsedTime);

        if (outputState)
        {
            Console.WriteLine($"Summary:");
            Console.WriteLine($"Overall Status: {solverResults.Status}");
            Console.WriteLine($"Total Duration: {solverResults.Duration.TotalMilliseconds:0.000}ms");
            Console.WriteLine();
            Console.WriteLine("Solutions:");
            for (var index = 0; index < solverResults.ExecutionResults.Count; index++)
            {
                Console.WriteLine($"({index + 1}) {solverResults.ExecutionResults[index]}");
            }
            Console.WriteLine();
        }

        if (generateSvg)
        {
            var count = 0;
            foreach (var state in states.Where(state => states.Count == 1 || state.Status != SolverStatus.Error))
            {
                count += 1;
                var grid = new HexGrid(state.Tableau.Grid.Size);

                grid.SetNodeCounts(state.Tableau.ChannelTileCounts);
                grid.SetInitialState(state.Tableau.GetNodeState());

                var options = new GridHtmlWriterOptions();
                var htmlWriter = new GridHtmlWriter(options);
                var filename = $"{state.Tableau.Tag}-{count}.html";
                var outputPath = htmlWriter.Write(grid, filename);

                if (outputState)
                {
                    Console.WriteLine($"Output written to:");
                    Console.WriteLine(outputPath);
                    Console.WriteLine();
                }
            }
        }

        return solverResults;
    }

    private static SolverResults GetSolverResults(List<SolverState> states, TimeSpan elapsedTime)
    {
        if (states.Count == 0)
        {
            return new SolverResults
            {
                Duration = elapsedTime,
                Status = SolverStatus.Unsolved,
                ExecutionResults = []
            };
        }

        if (states.Count == 1)
        {
            var state = states[0];

            return new SolverResults
            {
                Duration = elapsedTime,
                Status = state.Status,
                ExecutionResults = [state.ToSolverExecutionResult()]
            };
        }

        var solverCounts = states.Aggregate(
            new SolverCounts(),
            (counts, state) => counts.NotifyStatus(state.Status, state.ElapsedTime));

        var status = SolverStatus.Error;
        var statesToReturn = (IEnumerable<SolverState>)states;

        if (solverCounts.Unsolved > 0)
        {
            status = SolverStatus.Unsolved;
        }
        else if (solverCounts.Solved > 0)
        {
            status = SolverStatus.Solved;

            // Omit any results with Error status
            statesToReturn = states.Where(state => state.Status == SolverStatus.Solved);
        }

        return new SolverResults
        {
            Duration = elapsedTime,
            Status = status,
            ExecutionResults = [.. statesToReturn.Select(state => state.ToSolverExecutionResult())]
        };
    }
}
