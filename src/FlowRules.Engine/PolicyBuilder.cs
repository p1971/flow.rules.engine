using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FlowRules.Engine.Models;

namespace FlowRules.Engine;

/// <summary>
/// Utility class used to build policies.
/// </summary>
/// <typeparam name="T">The policy type to build.</typeparam>
public class PolicyBuilder<T>
    where T : class
{
    private readonly List<Rule<T>> _rules = [];

    private string? _id;
    private string? _name;
    private string? _description;

    /// <summary>
    /// Sets the Id of the policy.
    /// </summary>
    /// <param name="id">The id of the policy.</param>
    /// <returns>The current instance of the <see cref="PolicyBuilder{T}"/>.</returns>
    public PolicyBuilder<T> WithId(string id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the name of the policy.
    /// </summary>
    /// <param name="name">The name of the policy.</param>
    /// <returns>The current instance of the <see cref="PolicyBuilder{T}"/>.</returns>
    public PolicyBuilder<T> WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the description of the policy.
    /// </summary>
    /// <param name="description">The description of the policy.</param>
    /// <returns>The current instance of the <see cref="PolicyBuilder{T}"/>.</returns>
    public PolicyBuilder<T> WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Adds a rule to the policy.
    /// </summary>
    /// <param name="id">The id of the rule.</param>
    /// <param name="name">The name of the rule.</param>
    /// <param name="source">The source of the rule.</param>
    /// <param name="description">A description associated with the rule.</param>
    /// <param name="failureMessage">A failure message to emit if the rul fails.</param>
    /// <returns>The current instance of the <see cref="PolicyBuilder{T}"/>.</returns>
    public PolicyBuilder<T> WithRule(string id, string name, Func<T, CancellationToken, Task<bool>> source, string? description = null, Func<T, string>? failureMessage = null)
    {
        _rules.Add(new Rule<T>(id, name, description, failureMessage, source));
        return this;
    }

    /// <summary>
    /// Gets an instance of the <see cref="PolicyBuilder{T}"/>.
    /// </summary>
    public static PolicyBuilder<T> Instance
    {
        get
        {
            return new PolicyBuilder<T>();
        }
    }

    /// <summary>
    /// Builds the policy.
    /// </summary>
    /// <returns>An instance of the <see cref="Policy{T}"/>.</returns>
    public Policy<T> Build()
    {
        return new Policy<T>(_id!, _name!, _description, _rules);
    }
}
