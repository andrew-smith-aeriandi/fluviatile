using Solver.Framework;

namespace Solver.Rules;

[AttributeUsage(AttributeTargets.Class)]
public class RulePrioriryAttribute : Attribute
{
    public RulePrioriryAttribute(int value = QueuePriority.Default)
    {
        Value = value;
    }

    public int Value { get; }
}
