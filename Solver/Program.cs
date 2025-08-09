using Solver.Framework;
using Solver.Rules;

namespace Solver;

internal class Program
{
    private const int MaxRuleInvocations = 10000;
    private const int Size = 3;

    static void Main(string[] args)
    {
        var factory = new TableauFactory();
        var grid = new Grid(Size);

        //int[] channelCounts = [3, 6, 8, 10, 6, 3, 6, 6, 8, 8, 8, 0, 3, 6, 5, 8, 9, 5]; // Solved: True, Rule Invocations: 447, Reasons: 11
        //int[] channelCounts = [7, 5, 6, 8, 5, 7, 5, 8, 6, 10, 7, 2, 3, 6, 10, 8, 4, 7]; // Solved: True, Rule Invocations: 445, Reasons: 9
        //int[] channelCounts = [2, 4, 2, 8, 4, 3, 2, 5, 8, 8, 0, 0, 0, 5, 6, 4, 4, 4]; // Solved: True, Rule Invocations: 274, Reasons: 6
        //int[] channelCounts = [5, 7, 7, 8, 4, 5, 3, 7, 10, 10, 5, 1, 3, 4, 8, 8, 7, 6]; // Solved: True, Rule Invocations: 451, Reasons: 10
        //int[] channelCounts = [3, 6, 4, 8, 5, 1, 4, 4, 4, 8, 7, 0, 3, 4, 4, 7, 2, 7]; // Solved: True, Rule Invocations: 449, Reasons: 7
        //int[] channelCounts = [5, 2, 8, 7, 6, 5, 7, 7, 7, 7, 5, 0, 0, 5, 8, 9, 6, 5]; // Solved: True, Rule Invocations: 457, Reasons: 10
        //int[] channelCounts = [3, 7, 7, 7, 8, 4, 7, 6, 9, 7, 7, 0, 3, 5, 6, 9, 6, 7]; // Solved: True, Rule Invocations: 457, Reasons: 11
        //int[] channelCounts = [3, 6, 4, 5, 2, 6, 3, 4, 6, 10, 3, 0, 3, 4, 6, 2, 7, 4]; // Solved: True, Rule Invocations: 449, Reasons: 8
        //int[] channelCounts = [5, 4, 10, 10, 6, 3, 5, 8, 10, 6, 7, 2, 3, 5, 7, 9, 8, 6]; // Solved: True, Rule Invocations: 464, Reasons: 12
        //int[] channelCounts = [5, 6, 7, 8, 2, 6, 3, 7, 8, 10, 6, 0, 5, 4, 4, 7, 7, 7]; // Solved: True, Rule Invocations: 445, Reasons: 7
        //int[] channelCounts = [4, 6, 8, 10, 9, 3, 5, 8, 10, 9, 8, 0, 3, 6, 8, 9, 9, 5]; // Solved: True, Rule Invocations: 467, Reasons: 10
        //int[] channelCounts = [5, 7, 6, 6, 9, 4, 3, 7, 9, 9, 9, 0, 4, 6, 8, 6, 9, 4]; // Solved: True, Rule Invocations: 447, Reasons: 6
        //int[] channelCounts = [5, 6, 7, 9, 4, 5, 3, 6, 9, 11, 7, 0, 3, 6, 7, 8, 7, 5]; // Solved: True, Rule Invocations: 457, Reasons: 11
        //int[] channelCounts = [3, 9, 10, 7, 5, 4, 4, 6, 10, 9, 6, 3, 3, 7, 7, 7, 9, 5]; // Solved: True, Rule Invocations: 455, Reasons: 11
        //int[] channelCounts = [7, 5, 6, 8, 6, 7, 7, 5, 10, 10, 5, 2, 5, 4, 8, 8, 7, 7]; // Solved: True, Rule Invocations: 460, Reasons: 11
        //int[] channelCounts = [3, 7, 10, 6, 4, 5, 3, 9, 5, 7, 5, 6, 4, 8, 6, 7, 5, 5]; // Solved: True, Rule Invocations: 447, Reasons: 11
        //int[] channelCounts = [3, 7, 5, 6, 8, 2, 3, 2, 9, 10, 5, 2, 3, 5, 9, 7, 4, 3]; // Solved: True, Rule Invocations: 371, Reasons: 9
        //int[] channelCounts = [5, 6, 7, 8, 8, 6, 7, 4, 11, 11, 4, 3, 3, 6, 11, 9, 6, 5]; // Solved: True, Rule Invocations: 445, Reasons: 7
        //int[] channelCounts = [5, 4, 6, 8, 9, 3, 7, 4, 9, 8, 5, 2, 5, 2, 7, 10, 6, 5]; // Solved: True, Rule Invocations: 448, Reasons: 7
        //int[] channelCounts = [5, 4, 4, 8, 6, 6, 5, 6, 8, 8, 6, 0, 4, 6, 2, 10, 7, 4]; // Solved: True, Rule Invocations: 448, Reasons: 8
        //int[] channelCounts = [4, 4, 7, 7, 6, 3, 3, 7, 9, 2, 8, 2, 3, 6, 7, 5, 4, 6]; // Solved: True, Rule Invocations: 213, Reasons: 8
        //int[] channelCounts = [7, 6, 7, 7, 6, 7, 7, 5, 8, 9, 8, 3, 5, 5, 10, 6, 8, 6]; // not solved - very hard with hypotheticals required
        //int[] channelCounts = [4, 7, 10, 8, 6, 7, 6, 8, 9, 11, 5, 3, 3, 8, 10, 7, 8, 6]; // not solved
        //int[] channelCounts = [4, 8, 5, 6, 5, 7, 3, 7, 9, 7, 6, 3, 3, 7, 8, 9, 4, 4]; // not solved
        //int[] channelCounts = [5, 6, 8, 7, 5, 5, 6, 8, 8, 8, 6, 0, 0, 6, 8, 7, 8, 7]; // not solved
        //int[] channelCounts = [5, 7, 7, 8, 6, 7, 6, 8, 9, 8, 7, 2, 3, 6, 9, 10, 6, 6]; // not solved
        //int[] channelCounts = [3, 8, 8, 9, 5, 7, 7, 8, 8, 8, 7, 2, 3, 7, 8, 8, 9, 5]; // not solved
        int[] channelCounts = [5, 7, 8, 7, 8, 6, 7, 6, 10, 10, 5, 3, 4, 4, 11, 8, 9, 5];

        var tableau = factory.Create(grid, channelCounts);

        var rules = new List<IRule>
        {
            new AisleCountRule(),
            new MeanderRule(),
            new TileEdgeRule(),
            new AisleResolutionPatternRule(grid),
            new AisleCountIntersectionRule(grid),
            new ExitCountRule(grid),
            new ChannelContinuityRule(),
            new TarjansRule()
        };

        var state = new SolverState(tableau, rules, new HousekeepingRule(tableau));
        var isSolved = false;
        var ruleInvocations = 0;

        try
        {
            isSolved = state.Solve(MaxRuleInvocations, out ruleInvocations);
        }
        catch (Exception ex)
        {
            var foregroundColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            Console.WriteLine();
            Console.ForegroundColor = foregroundColour;
        }

        foreach (var aisle in tableau.Aisles.Values.OrderBy(a => a.Axis).ThenBy(a => a.Index))
        {
            Console.WriteLine(aisle.ToString());
        }
        Console.WriteLine();

        foreach (var tile in tableau.Tiles.Values.OrderBy(t => t.AisleX.Index).ThenBy(t => t.AisleY.Index).ThenByDescending(t => t.AisleZ.Index))
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
}
