using Solver.Rules;

namespace Solver.Framework;

public readonly struct RuleType : IEquatable<RuleType>
{
    public readonly static RuleType Default = new();

    public static RuleType Create<T>() where T : Rule
    {
        return new RuleType(typeof(T));
    }

    public RuleType() : this(null)
    {
    }

    public RuleType(Type? type)
    {
        type ??= typeof(NoopRule);
        
        if (!type.IsSubclassOf(typeof(Rule)))
        {
            throw new ArgumentException(
                $"Type {type.FullName} must derive from {typeof(Rule).FullName}.",
                nameof(type));
        }

        Type = type;
    }

    public Type Type { get; }

    public bool Equals(RuleType other)
    {
        return other.Type == Type;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is RuleType ruleType && ruleType.Type == Type;
    }

    public static bool operator ==(RuleType left, RuleType right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RuleType left, RuleType right)
    {
        return !left.Equals(right);
    }

    public static implicit operator Type(RuleType ruleType)
    {
        return ruleType.Type;
    }

    public override readonly int GetHashCode()
    {
        return Type.GetHashCode();
    }

    public override readonly string ToString()
    {
        return Type.Name;
    }
}
