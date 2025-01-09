using System.Collections.Generic;

namespace FlowRules.Engine.Models;

/// <summary>
/// Represents a rules policy.
/// </summary>
/// <typeparam name="T">The type the policy is to be executed against.</typeparam>
public class Policy<T>(string id, string name, string? description, IList<Rule<T>> rules)
    where T : class
{
    /// <summary>
    /// Gets the id of the policy.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Gets the name of the policy.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the description of the policy.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Gets the rules of the policy.
    /// </summary>
    public IList<Rule<T>> Rules { get; } = rules;
}
