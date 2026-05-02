using Solver.Components;
using Solver.Rules;

namespace Solver.Framework;

public readonly struct ComponentType : IEquatable<ComponentType>
{
    public readonly static ComponentType Default = new();

    public static ComponentType Create<T>() where T : IComponent
    {
        return new ComponentType(typeof(T));
    }

    public ComponentType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!type.IsAssignableTo(typeof(IComponent)))
        {
            throw new ArgumentException(
                $"Type {type.FullName} must derive from {typeof(Rule).FullName}.",
                nameof(type));
        }

        Type = type;
    }

    public Type Type { get; }

    public bool Equals(ComponentType other)
    {
        return other.Type == Type;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is ComponentType ruleType && ruleType.Type == Type;
    }

    public static bool operator ==(ComponentType left, ComponentType right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ComponentType left, ComponentType right)
    {
        return !left.Equals(right);
    }

    public static implicit operator Type(ComponentType componentType)
    {
        return componentType.Type;
    }

    public override readonly int GetHashCode()
    {
        return Type?.GetHashCode() ?? 0;
    }

    public override readonly string ToString()
    {
        return Type?.Name ?? "(null)";
    }
}
