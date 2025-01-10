using System;

namespace FlowRules.Engine.Models;

/// <summary>
/// Encapsulates the result of the running the policy.
/// </summary>
public class PolicyExecutionResult
{
    /// <summary>
    /// Gets the name of the policy.
    /// </summary>
    public string? PolicyName { get; init; }

    /// <summary>
    /// Gets the policy id.
    /// </summary>
    public string? PolicyId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the policy passed.
    /// </summary>
    public bool Passed { get; init; }

    /// <summary>
    /// Gets the individual rule execution results.
    /// </summary>
    public RuleExecutionResult[] RuleExecutionResults { get; init; } = [];

    /// <summary>
    /// Gets the rule execution context id.
    /// </summary>
    public Guid RuleContextId { get; init; }

    /// <summary>
    /// Gets the correlation id to track requests from callers.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the version of the rules policy.
    /// </summary>
    public string? Version { get; init; }
}
