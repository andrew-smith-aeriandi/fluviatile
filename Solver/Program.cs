using Fluviatile.Grid;
using Fluviatile.Grid.Random;
using GridWriter;
using GridWriter.Settings;
using Solver.Components;
using Solver.Framework;
using Solver.Rules;
using Tableau = Solver.Components.Tableau;

namespace Solver;

internal partial class Program
{
    private const int Size = 3;
    private readonly static SolverGrid SolverGrid = new(Size);

    private readonly static Type HousekeepingRule = typeof(HousekeepingRule);
    private readonly static Type[] Rules =
    [
        typeof(AisleCountRule),
        typeof(TileEdgeRule),
        typeof(MeanderRule),
        typeof(AisleResolutionPatternRule),
        typeof(AisleCountIntersectionRule),
        typeof(ExitCountRule),
        typeof(ChannelContinuityRule),
        typeof(TarjansRule)
    ];

    internal async static Task Main(string[] args)
    {
        var options = new SolverOptions
        {
            MaxHypotheticals = args.GetInteger("--max-hypotheticals", SolverOptions.Default.MaxHypotheticals),
            MaxRuleInvocations = args.GetInteger("--max-rules", SolverOptions.Default.MaxRuleInvocations)
        };

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

            Solve(
                new Puzzle(nodeCounts, SolverStatus.Unsolved, "Random"),
                options);
        }
    }

    private static void Solve(Puzzle puzzle, SolverOptions options)
    {
        var rulesetFactory = RulesetFactory.Create(HousekeepingRule, Rules);
        var runner = new SolverRunner(rulesetFactory, options);
        var tableau = TableauFactory.Create(SolverGrid, puzzle.ChannelCounts);

        Solve(
            runner: runner,
            tableau: tableau,
            outputState: true,
            generateSvg: true);
    }

    private static void Solve(IEnumerable<Puzzle> puzzles, SolverOptions options)
    {
        var rulesetFactory = RulesetFactory.Create(HousekeepingRule, Rules);
        var runner = new SolverRunner(rulesetFactory, options);

        var solverCounts = new SolverCounts();
        var index = 0;

        foreach (var puzzle in puzzles)
        {
            var tag = $"tableau-{index}";
            var tableau = TableauFactory.Create(SolverGrid, puzzle.ChannelCounts, tag);

            var state = new SolverState(tableau, rulesetFactory);

            var results = Solve(
                runner: runner,
                tableau: tableau,
                outputState: false,
                generateSvg: false);

            if (results.TryGetUniqueSolution(out var uniqueResult))
            {
                Console.WriteLine($"{index}: {tableau} => {uniqueResult}");
            }
            else
            {
                Console.WriteLine($"{index}: {tableau} => {results}");
            }

            solverCounts.NotifyStatus(results.Status);
            index += 1;
        }

        Console.WriteLine(solverCounts.ToString());
    }

    private static SolverResults Solve(
        SolverRunner runner,
        Tableau tableau,
        bool outputState = false,
        bool generateSvg = false)
    {
        var states = runner.Solve(tableau).ToList();

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

                Console.WriteLine($"Output written to:\n{outputPath}");
            }
        }

        return GetSolverResults(states);
    }

    private static SolverResults GetSolverResults(List<SolverState> states)
    {
        if (states.Count == 0)
        {
            return new SolverResults
            {
                Status = SolverStatus.Unsolved,
                ExecutionResults = []
            };
        }

        if (states.Count == 1)
        {
            var state = states[0];

            return new SolverResults
            {
                Status = state.Status,
                ExecutionResults = [state.ToSolverExecutionResult()]
            };
        }

        var solverCounts = states.Aggregate(
            new SolverCounts(),
            (counts, state) => counts.NotifyStatus(state.Status));

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
            Status = status,
            ExecutionResults = [.. statesToReturn.Select(state => state.ToSolverExecutionResult())]
        };
    }
}
