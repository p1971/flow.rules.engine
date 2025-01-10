using System;

namespace FlowRules.Engine.Models;

/// <summary>
/// Represents the results of executing a rule as part of a policy.
/// </summary>
public class RuleExecutionResult(string id, string name, string? description)
{
    /// <summary>
    /// Gets the id of the rule.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Gets the name of the rule.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the description of the rule.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Gets or sets a value indicating whether the rule passed.
    /// </summary>
    public bool Passed { get; set; }

    /// <summary>
    /// Gets or sets the failure message for the rule.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the time taken to execute the rule.
    /// </summary>
    public TimeSpan Elapsed { get; set; }

    /// <summary>
    /// Gets or sets any exception associated with the rule.
    /// </summary>
    public Exception? Exception { get; set; }
}
