namespace Solver.Components;

public interface ILinkable : ILocatable
{
    bool IsTerminal { get; }
}
