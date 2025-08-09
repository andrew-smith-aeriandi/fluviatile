using Solver.Components;
using Solver.Rules;

namespace Solver.Framework;

public readonly record struct RuleInvocation(IRule Rule, IComponent Component);
