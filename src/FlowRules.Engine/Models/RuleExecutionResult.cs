using System;

namespace FlowRules.Engine.Models;

/// <summary>
/// Represents the results of executing a rule as part of a policy.
/// </summary>
public record RuleExecutionResult(
    string Id,
    string Name,
    bool Passed,
    TimeSpan Elapsed,
    string? Description,
    string? Message,
    Exception? Exception);
