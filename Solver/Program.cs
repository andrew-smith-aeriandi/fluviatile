using Fluviatile.Grid;
using GridWriter;
using GridWriter.Settings;
using Solver.Components;
using Solver.Framework;
using Solver.Rules;

namespace Solver;

internal class Program
{
    private const int Size = 3;
    private const int MaxRuleInvocations = 10000;

    private readonly static SolverGrid SolverGrid;
    private readonly static IReadOnlyList<IRule> Rules;

    static Program()
    {
        SolverGrid = new SolverGrid(Size);
        Rules =
        [
            new AisleCountRule(),
            new TileEdgeRule(),
            new MeanderRule(),
            new AisleResolutionPatternRule(SolverGrid),
            new AisleCountIntersectionRule(SolverGrid),
            new ExitCountRule(SolverGrid),
            new ChannelContinuityRule(),
            new TarjansRule()
        ];
    }

    internal static void Main(string[] args)
    {
        var cmd = args.Length > 0
            ? args[0].ToLowerInvariant()
            : "all";

        if (int.TryParse(cmd, out var index))
        {
            if (index >= 0)
            {

                Solve(Gallery.GetByIndex(index));
            }
            else
            {
                Solve(Gallery.GetByIndex(^1));
            }
        }
        else
        {
            var puzzles = cmd switch
            {
                "solved" => Gallery.GetAllSolved(),
                "unsolved" => Gallery.GetAllInsolved(),
                _ => Gallery.GetAll()
            };

            var (solved, unsolved) = Solve(puzzles);
            Console.WriteLine($"Solved: {solved}, Unsolved: {unsolved}");
        }
    }

    private static bool Solve(Puzzle puzzle)
    {
        var tableau = TableauFactory.Create(SolverGrid, puzzle.ChannelCounts);
        var state = new SolverState(tableau, Rules, new HousekeepingRule(tableau));

        return Solve(
            state: state,
            outputState: true,
            generateSvg: true);
    }

    private static (int SolvedCount, int UnsolvedCount) Solve(IEnumerable<Puzzle> puzzles)
    {
        var solvedCount = 0;
        var unsolvedCount = 0;

        foreach (var puzzle in puzzles)
        {
            var tableau = TableauFactory.Create(SolverGrid, puzzle.ChannelCounts);
            var state = new SolverState(tableau, Rules, new HousekeepingRule(tableau));

            if (Solve(state))
            {
                Console.WriteLine($"{tableau}=>Solved");
                solvedCount++;
            }
            else
            {
                Console.WriteLine($"{tableau}=>Unsolved");
                unsolvedCount++;
            }
        }

        return (solvedCount, unsolvedCount);
    }

    private static bool Solve(
        SolverState state,
        bool outputState = false,
        bool generateSvg = false)
    {
        var tableau = state.Tableau;
        var isSolved = false;
        var ruleInvocations = 0;

        try
        {
            isSolved = state.Solve(MaxRuleInvocations, out ruleInvocations);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{string.Join(", ", tableau.ChannelCounts.Select(n => n.ToString()))}]");

            var foregroundColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{ex.GetType().Name}: {ex}");
            Console.WriteLine();
            Console.ForegroundColor = foregroundColour;
        }

        if (outputState)
        {
            foreach (var aisle in tableau.GetAisles()
                .OrderBy(a => a.Axis)
                .ThenBy(a => a.Index))
            {
                Console.WriteLine(aisle.ToString());
            }
            Console.WriteLine();

            foreach (var tile in tableau.GetTiles()
                .OrderBy(t => t.AisleX.Index)
                .ThenBy(t => t.AisleY.Index)
                .ThenByDescending(t => t.AisleZ.Index))
            {
                Console.WriteLine(tile.ToString());
            }
            Console.WriteLine();

            Console.WriteLine(tableau.Thalweg.ToString());
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

            Console.WriteLine($"Solved: {isSolved}, Rule Invocations: {ruleInvocations}, Reasons: {reasons.Length}");
            Console.WriteLine();
        }

        if (generateSvg)
        {
            var grid = new HexGrid(tableau.Grid.Size);

            grid.SetNodeCounts(tableau.ChannelCounts);
            grid.SetInitialState(tableau.GetNodeState());

            var options = new GridHtmlWriterOptions();
            var htmlWriter = new GridHtmlWriter(options);
            var outputPath = htmlWriter.Write(grid);

            Console.WriteLine($"Output written to:\n{outputPath}");
        }

        return isSolved;
    }
}
