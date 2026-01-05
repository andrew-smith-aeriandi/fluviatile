using Solver.Framework;

namespace Solver;

public static class Gallery
{
    private readonly static List<Puzzle> Inventory =
    [
        new([3, 6, 8, 10, 6, 3, 6, 6, 8, 8, 8, 0, 3, 6, 5, 8, 9, 5], SolverResult.Solved, "Rule Invocations: 447, Reasons: 11"),
        new([7, 5, 6, 8, 5, 7, 5, 8, 6, 10, 7, 2, 3, 6, 10, 8, 4, 7], SolverResult.Solved, "Rule Invocations: 445, Reasons: 9"),
        new([2, 4, 2, 8, 4, 3, 2, 5, 8, 8, 0, 0, 0, 5, 6, 4, 4, 4], SolverResult.Solved, "Rule Invocations: 274, Reasons: 6"),
        new([5, 7, 7, 8, 4, 5, 3, 7, 10, 10, 5, 1, 3, 4, 8, 8, 7, 6], SolverResult.Solved, "Rule Invocations: 451, Reasons: 10"),
        new([3, 6, 4, 8, 5, 1, 4, 4, 4, 8, 7, 0, 3, 4, 4, 7, 2, 7], SolverResult.Solved, "Rule Invocations: 449, Reasons: 7"),
        new([5, 2, 8, 7, 6, 5, 7, 7, 7, 7, 5, 0, 0, 5, 8, 9, 6, 5], SolverResult.Solved, "Rule Invocations: 457, Reasons: 10"),
        new([3, 7, 7, 7, 8, 4, 7, 6, 9, 7, 7, 0, 3, 5, 6, 9, 6, 7], SolverResult.Solved, "Rule Invocations: 457, Reasons: 11"),
        new([3, 6, 4, 5, 2, 6, 3, 4, 6, 10, 3, 0, 3, 4, 6, 2, 7, 4], SolverResult.Solved, "Rule Invocations: 449, Reasons: 8"),
        new([5, 4, 10, 10, 6, 3, 5, 8, 10, 6, 7, 2, 3, 5, 7, 9, 8, 6], SolverResult.Solved, "Rule Invocations: 464, Reasons: 12"),
        new([5, 6, 7, 8, 2, 6, 3, 7, 8, 10, 6, 0, 5, 4, 4, 7, 7, 7], SolverResult.Solved, "Rule Invocations: 445, Reasons: 7"),
        new([4, 6, 8, 10, 9, 3, 5, 8, 10, 9, 8, 0, 3, 6, 8, 9, 9, 5], SolverResult.Solved, "Rule Invocations: 467, Reasons: 10"),
        new([5, 7, 6, 6, 9, 4, 3, 7, 9, 9, 9, 0, 4, 6, 8, 6, 9, 4], SolverResult.Solved, "Rule Invocations: 447, Reasons: 6"),
        new([5, 6, 7, 9, 4, 5, 3, 6, 9, 11, 7, 0, 3, 6, 7, 8, 7, 5], SolverResult.Solved, "Rule Invocations: 457, Reasons: 11"),
        new([3, 9, 10, 7, 5, 4, 4, 6, 10, 9, 6, 3, 3, 7, 7, 7, 9, 5], SolverResult.Solved, "Rule Invocations: 455, Reasons: 11"),
        new([7, 5, 6, 8, 6, 7, 7, 5, 10, 10, 5, 2, 5, 4, 8, 8, 7, 7], SolverResult.Solved, "Rule Invocations: 460, Reasons: 11"),
        new([3, 7, 10, 6, 4, 5, 3, 9, 5, 7, 5, 6, 4, 8, 6, 7, 5, 5], SolverResult.Solved, "Rule Invocations: 447, Reasons: 11"),
        new([3, 7, 5, 6, 8, 2, 3, 2, 9, 10, 5, 2, 3, 5, 9, 7, 4, 3], SolverResult.Solved, "Rule Invocations: 371, Reasons: 9"),
        new([5, 6, 7, 8, 8, 6, 7, 4, 11, 11, 4, 3, 3, 6, 11, 9, 6, 5], SolverResult.Solved, "Rule Invocations: 445, Reasons: 7"),
        new([5, 4, 6, 8, 9, 3, 7, 4, 9, 8, 5, 2, 5, 2, 7, 10, 6, 5], SolverResult.Solved, "Rule Invocations: 448, Reasons: 7"),
        new([5, 4, 4, 8, 6, 6, 5, 6, 8, 8, 6, 0, 4, 6, 2, 10, 7, 4], SolverResult.Solved, "Rule Invocations: 448, Reasons: 8"),
        new([4, 4, 7, 7, 6, 3, 3, 7, 9, 2, 8, 2, 3, 6, 7, 5, 4, 6], SolverResult.Solved, "Rule Invocations: 213, Reasons: 8"),
        new([3, 6, 6, 11, 5, 5, 4, 6, 8, 7, 7, 4, 5, 6, 10, 6, 6, 3], SolverResult.Solved, "Rule Invocations: 456, Reasons: 9"),
        new([0, 0, 5, 6, 5, 6, 3, 6, 4, 6, 2, 1, 5, 4, 4, 6, 3, 0], SolverResult.Solved, "Rule Invocations: 450, Reasons: 10"),
        new([2, 7, 6, 8, 6, 3, 3, 6, 6, 6, 4, 7, 6, 7, 5, 5, 6, 3], SolverResult.Solved, "Rule Invocations: 458, Reasons: 12"),
        new([2, 7, 10, 9, 4, 3, 3, 4, 7, 8, 8, 5, 7, 7, 4, 7, 7, 3], SolverResult.Solved, "Rule Invocations: 454, Reasons: 12"),
        new([2, 4, 10, 2, 2, 7, 7, 4, 4, 3, 5, 4, 2, 5, 6, 7, 4, 3], SolverResult.Solved, "Rule Invocations: 448, Reasons: 8"),
        new([0, 7, 8, 6, 8, 5, 7, 4, 5, 8, 7, 3, 5, 7, 9, 4, 6, 3], SolverResult.Solved, "Rule Invocations: 458, Reasons: 11"),
        new([0, 9, 7, 10, 7, 5, 4, 7, 7, 8, 7, 5, 7, 9, 8, 5, 4, 5], SolverResult.Solved, "Rule Invocations: 462, Reasons: 11"),
        new([1, 5, 9, 6, 4, 5, 4, 5, 8, 4, 4, 5, 3, 7, 9, 4, 3, 4], SolverResult.Solved, "Rule Invocations: 257, Reasons: 8"),
        new([4, 3, 10, 9, 7, 5, 6, 7, 6, 8, 7, 4, 5, 7, 8, 8, 6, 4], SolverResult.Solved, "Rule Invocations: 357, Reasons: 8"),
        new([3, 6, 9, 7, 3, 5, 4, 7, 8, 9, 5, 0, 2, 5, 6, 7, 7, 6], SolverResult.Solved, "Rule Invocations: 456, Reasons: 11"),
        new([6, 5, 8, 7, 8, 6, 4, 9, 9, 11, 5, 2, 3, 7, 8, 10, 7, 5], SolverResult.Solved, "Rule Invocations: 446, Reasons: 9"),
        new([6, 8, 8, 7, 4, 5, 3, 7, 8, 10, 8, 2, 4, 5, 10, 4, 8, 7], SolverResult.Solved, "Rule Invocations: 444, Reasons: 9"),
        new([5, 4, 9, 7, 5, 3, 2, 6, 11, 8, 6, 0, 2, 6, 5, 8, 6, 6], SolverResult.Solved, "Rule Invocations: 450, Reasons: 6"),
        new([7, 8, 5, 6, 9, 7, 5, 9, 10, 7, 6, 5, 7, 4, 7, 10, 9, 5], SolverResult.Solved, "Rule Invocations: 456, Reasons: 9"),
        new([3, 6, 8, 5, 7, 3, 7, 5, 6, 10, 4, 0, 2, 2, 9, 6, 6, 7], SolverResult.Solved, "Rule Invocations: 458, Reasons: 11"),
        new([5, 2, 7, 6, 8, 4, 7, 6, 7, 8, 2, 2, 2, 2, 8, 9, 8, 3], SolverResult.Solved, "Rule Invocations: 448, Reasons: 10"),
        new([7, 8, 4, 10, 8, 6, 7, 8, 10, 8, 7, 3, 4, 6, 9, 8, 9, 7], SolverResult.Solved, "Rule Invocations: 454, Reasons: 9"),
        new([3, 8, 8, 4, 5, 4, 4, 3, 9, 9, 4, 3, 3, 7, 7, 2, 7, 6], SolverResult.Solved, "Rule Invocations: 454, Reasons: 10"),
        new([5, 6, 8, 7, 5, 5, 6, 8, 8, 8, 6, 0, 0, 6, 8, 7, 8, 7], SolverResult.Solved, "Rule Invocations: 454, Reasons: 9 (Tarjans Rule)"),
        new([4, 6, 5, 8, 8, 7, 5, 9, 10, 7, 7, 0, 3, 7, 8, 7, 9, 4], SolverResult.Solved, "Rule Invocations: 441, Reasons: 8"),
        new([2, 7, 6, 7, 8, 3, 6, 7, 7, 7, 6, 0, 2, 2, 9, 10, 5, 5], SolverResult.Solved, "Rule Invocations: 449, Reasons: 10"),
        new([4, 8, 6, 4, 7, 6, 3, 7, 9, 8, 5, 3, 3, 8, 6, 8, 5, 5], SolverResult.Solved, "Rule Invocations: 385, Reasons: 9"),
        new([3, 4, 4, 6, 6, 5, 6, 7, 7, 8, 0, 0, 2, 3, 4, 8, 4, 7], SolverResult.Solved, "Rule Invocations: 435, Reasons: 8"),
        new([0, 8, 6, 9, 8, 7, 5, 9, 8, 7, 6, 3, 5, 8, 8, 8, 7, 2], SolverResult.Solved, "Rule Invocations: 462, Reasons: 11"),
        new([6, 7, 6, 9, 7, 6, 6, 8, 10, 9, 7, 1, 2, 7, 8, 10, 7, 7], SolverResult.Solved, "Rule Invocations: 458, Reasons: 13"),
        new([5, 4, 6, 9, 7, 2, 3, 7, 9, 8, 6, 0, 4, 5, 4, 7, 6, 7], SolverResult.Solved, "Rule Invocations: 458, Reasons: 12 satisfying"),
        new([6, 4, 4, 6, 4, 7, 3, 8, 4, 9, 6, 1, 5, 4, 4, 9, 4, 5], SolverResult.Solved, "Rule Invocations: 445, Reasons: 8"),
        new([5, 7, 7, 8, 9, 1, 4, 7, 8, 9, 9, 0, 3, 4, 7, 9, 9, 5], SolverResult.Solved, "Rule Invocations: 442, Reasons: 7, Hypotheticals: 0"),
        new([5, 7, 8, 7, 8, 6, 7, 6, 10, 10, 5, 3, 4, 4, 11, 8, 9, 5], SolverResult.Solved, "Rule Invocations: 483, Reasons: 13, Hypotheticals: 1"),
        new([4, 7, 10, 8, 6, 7, 6, 8, 9, 11, 5, 3, 3, 8, 10, 7, 8, 6], SolverResult.Solved, "Rule Invocations: 448, Reasons: 12, Hypotheticals: 1"),
        new([3, 8, 8, 9, 5, 7, 7, 8, 8, 8, 7, 2, 3, 7, 8, 8, 9, 5], SolverResult.Solved, "Rule Invocations: 463, Reasons: 11, Hypotheticals: 1"),
        new([5, 6, 7, 8, 7, 5, 5, 7, 8, 6, 8, 4, 5, 5, 10, 9, 2, 7], SolverResult.Solved, "Rule Invocations: 471, Reasons: 9, Hypotheticals: 1"),
        new([5, 5, 10, 6, 4, 6, 3, 7, 8, 8, 7, 3, 4, 6, 8, 8, 5, 5], SolverResult.Solved, "Rule Invocations: 471, Reasons: 12, Hypotheticals: 1"), // 48
        new([5, 4, 5, 6, 8, 7, 7, 7, 11, 7, 3, 0, 3, 4, 5, 10, 8, 5], SolverResult.Solved, "Rule Invocations: 467, Reasons: 10, Hypotheticals: 1"),
        new([5, 6, 4, 6, 8, 5, 3, 9, 10, 8, 4, 0, 2, 5, 6, 9, 7, 5], SolverResult.Solved, "Rule Invocations: 483, Reasons: 13, Hypotheticals: 1"),
        new([6, 7, 8, 10, 9, 6, 7, 8, 11, 10, 7, 3, 4, 6, 11, 11, 8, 6], SolverResult.Solved, "Rule Invocations: 480, Reasons: 10, Hypotheticals: 1"),
        new([2, 9, 6, 10, 6, 6, 7, 6, 8, 7, 6, 5, 5, 6, 9, 9, 5, 5], SolverResult.Solved, "Rule Invocations: 471, Reasons: 12, Hypotheticals: 2"),
        new([6, 4, 7, 7, 6, 5, 7, 4, 7, 8, 7, 2, 3, 6, 7, 8, 5, 6], SolverResult.Solved, "Rule Invocations: 429, Reasons: 12, Hypotheticals: 3"),
        new([4, 8, 5, 6, 5, 7, 3, 7, 9, 7, 6, 3, 3, 7, 8, 9, 4, 4], SolverResult.Solved, "Rule Invocations: 355, Reasons: 11, Hypotheticals: 4"),
        new([5, 7, 7, 9, 7, 3, 5, 7, 8, 8, 8, 2, 3, 4, 9, 10, 7, 5], SolverResult.Solved, "Rule Invocations: 223, Reasons: 8, Hypotheticals: 5"),
        new([5, 6, 8, 7, 6, 5, 5, 6, 9, 10, 5, 2, 3, 8, 5, 8, 6, 7], SolverResult.Solved, "Rule Invocations: 267, Reasons: 9, Hypotheticals: 5"),
        new([5, 7, 7, 8, 6, 7, 6, 8, 9, 8, 7, 2, 3, 6, 9, 10, 6, 6], SolverResult.Solved, "Rule Invocations: 445, Reasons: 9, Hypotheticals: 5"),
        new([7, 6, 7, 7, 6, 7, 7, 5, 8, 9, 8, 3, 5, 5, 10, 6, 8, 6], SolverResult.Solved, "Rule Invocations: 360, Reasons: 9, Hypotheticals: 5"),
        new([5, 7, 2, 6, 7, 6, 5, 8, 8, 5, 4, 3, 3, 4, 7, 8, 6, 5], SolverResult.Solved, "Rule Invocations: 546, Reasons: 10, Hypotheticals: 6"),
        new([6, 6, 9, 8, 6, 5, 5, 8, 8, 9, 8, 2, 4, 5, 8, 9, 8, 6], SolverResult.Solved, "Rule Invocations: 330, Reasons: 10, Hypotheticals: 9"),
        new([5, 6, 9, 9, 7, 5, 5, 9, 8, 7, 6, 6, 6, 6, 8, 7, 9, 5], SolverResult.Solved, "Rule Invocations: 411, Reasons: 7, Hypotheticals: 16"),
        new([6, 8, 8, 7, 8, 6, 6, 6, 9, 10, 7, 5, 5, 8, 9, 8, 7, 6], SolverResult.Solved, "Rule Invocations: 345, Reasons: 8, Hypotheticals: 20"),
        new([7, 8, 7, 9, 4, 7, 6, 8, 7, 9, 8, 4, 5, 5, 8, 10, 7, 7], SolverResult.Solved, "Rule Invocations: 507, Reasons: 13, Hypotheticals: 6"),
     ];

    public static Puzzle GetByIndex(Index index)
    {
        return Inventory[index];
    }

    public static IEnumerable<Puzzle> GetAll()
    {
        return Inventory;
    }

    public static IEnumerable<Puzzle> GetAllSolved()
    {
        return Inventory.Where(exhibit => exhibit.SolverResult == SolverResult.Solved);
    }

    public static IEnumerable<Puzzle> GetAllUnsolved()
    {
        return Inventory.Where(exhibit => exhibit.SolverResult == SolverResult.Unsolved);
    }
}
