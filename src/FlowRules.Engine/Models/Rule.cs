using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlowRules.Engine.Models;

/// <summary>
/// Represents a rule that will be executed as part of the policy.
/// </summary>
/// <typeparam name="T">The type to execute against.</typeparam>
public class Rule<T>(string id, string name, string? description, Func<T, string>? failureMessage, Func<T, CancellationToken, Task<bool>> source)
    where T : class
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
    /// Gets the function that returns a failure message for the rule.
    /// </summary>
    public Func<T, string>? FailureMessage { get; } = failureMessage;

    /// <summary>
    /// Gets the source of the rule.
    /// </summary>
    public Func<T, CancellationToken, Task<bool>> Source { get; } = source;
}
