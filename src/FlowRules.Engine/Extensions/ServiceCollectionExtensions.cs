using System;

using FlowRules.Engine.Interfaces;
using FlowRules.Engine.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FlowRules.Engine.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for the FlowRules.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the FlowRules dependencies to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type that the rules will execute against.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="policyAction">A function to return the policy.</param>
    /// <param name="setupAction">A setup function for the options.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddFlowRules<T>(
        this IServiceCollection services,
        Func<Policy<T>> policyAction,
        Action<FlowRulesOptions>? setupAction = null)
        where T : class
    {
        FlowRulesOptions options = new();

        setupAction?.Invoke(options);

        Policy<T> policy = policyAction();
        services.AddSingleton(policy);

        if (options.ResultsRepository != null)
        {
            services.TryAddSingleton(typeof(IPolicyResultsRepository<T>), options.ResultsRepository);
        }
        else
        {
            services.TryAddSingleton<IPolicyResultsRepository<T>, DefaultPolicyResultsRepository<T>>();
        }

        services.AddSingleton<IPolicyManager<T>, PolicyManager<T>>();

        return services;
    }
}
