using Fluviatile.Grid;
using GridWriter;
using GridWriter.Settings;
using Solver.Components;
using Solver.Framework;
using Solver.Rules;
using Tableau = Solver.Components.Tableau;

namespace Solver;

internal class Program
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

    internal static void Main(string[] args)
    {
        var options = new SolverOptions
        {
            MaxHypotheticals = args.GetInteger("--max-hypotheticals", SolverOptions.Default.MaxHypotheticals),
            MaxRuleInvocations = args.GetInteger("--max-rules", SolverOptions.Default.MaxRuleInvocations)
        };

        var galleryString = args.GetString("--gallery", "all");
        var gallery = galleryString;
        var index = Index.FromEnd(1);
        var useIndex = false;

        var separator = gallery.IndexOf(':');
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

    private static void Solve(Puzzle puzzle, SolverOptions options)
    {
        var runner = new SolverRunner(
            RulesetFactory.Create(HousekeepingRule, Rules),
            options);

        Solve(
            runner: runner,
            tableau: TableauFactory.Create(SolverGrid, puzzle.ChannelCounts),
            outputState: true,
            generateSvg: true);
    }

    private static void Solve(IEnumerable<Puzzle> puzzles, SolverOptions options)
    {
        var solvedCount = 0;
        var unsolvedCount = 0;
        var errorCount = 0;

        var runner = new SolverRunner(
            RulesetFactory.Create(HousekeepingRule, Rules),
            options);

        var index = 0;

        foreach (var puzzle in puzzles)
        {
            var tag = $"tableau-{index}";

            var state = new SolverState(
                TableauFactory.Create(SolverGrid, puzzle.ChannelCounts, tag),
                RulesetFactory.Create(HousekeepingRule, Rules));

            var result = Solve(
                runner: runner,
                tableau: state.Tableau,
                outputState: false,
                generateSvg: true);

            Console.WriteLine($"{index}: {state.Tableau}=>{result}");
            index += 1;

            switch (result)
            {
                case SolverResult.Solved:
                    solvedCount++;
                    break;
                case SolverResult.Unsolved:
                    unsolvedCount++;
                    break;
                case SolverResult.Error:
                    errorCount++;
                    break;
            }
        }

        Console.WriteLine($"Solved: {solvedCount}, Unsolved: {unsolvedCount}, Error: {errorCount}");
    }

    private static SolverResult Solve(
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

                if (state.Result == SolverResult.Error)
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

                Console.WriteLine($"{state.Result}: Rule Invocations: {state.RuleInvocationCount}, Reasons: {reasons.Length}, Hypotheticals: {state.HypotheticalComponentsCount}");
                Console.WriteLine();
            }
        }


        if (generateSvg)
        {
            var count = 0;
            foreach (var state in states.Where(state => state.Result != SolverResult.Unsolved))
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

        var solvedCount = states.Count(state => state.Result == SolverResult.Solved);

        return solvedCount > 0
            ? SolverResult.Solved
            : SolverResult.Unsolved;
    }
}
