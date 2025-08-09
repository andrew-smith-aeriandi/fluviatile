namespace Solver.Components;

public interface IFreezable
{
    bool IsFrozen { get; }

    void Freeze();
}
