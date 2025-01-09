using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FlowRules.Engine.Interfaces;
using FlowRules.Engine.Models;

using Microsoft.Extensions.Logging;

namespace FlowRules.Engine;

/// <inheritdoc />
public class PolicyManager<T>(Policy<T> policy, IPolicyResultsRepository<T> resultsRepository, ILogger<PolicyManager<T>> logger) : IPolicyManager<T>
    where T : class
{
    /// <inheritdoc />
    public async Task<PolicyExecutionResult> Execute(
        string correlationId,
        Guid executionContextId,
        T request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing [{policyId}]:[{policyName}] for [{executionContextId}]",
            policy.Id,
            policy.Name,
            executionContextId);

        Stopwatch stopwatch = Stopwatch.StartNew();

        IList<RuleExecutionResult> response = await Execute(policy, executionContextId, request, cancellationToken);

        PolicyExecutionResult policyExecutionResult =
            new()
            {
                RuleContextId = executionContextId,
                CorrelationId = correlationId,
                PolicyId = policy.Id,
                PolicyName = policy.Name,
                Version = policy.GetType().Assembly.GetName().Version?.ToString(4),
                RuleExecutionResults = [..response],
                Passed = response.All(r => r.Passed)
            };

        stopwatch.Stop();

        await TryPersistResults(request, policyExecutionResult);

        FlowRulesEventCounterSource.EventSource.PolicyExecution(policy.Id, stopwatch.ElapsedMilliseconds);

        return policyExecutionResult;
    }

    /// <inheritdoc />
    public async Task<RuleExecutionResult> Execute(
        string ruleId,
        string correlationId,
        Guid executionContextId,
        T request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing [{ruleId}] for [{executionContextId}]",
            ruleId,
            executionContextId);

        Rule<T> rule = policy.Rules.FirstOrDefault(r => r.Id == ruleId)
            ?? throw new InvalidOperationException($"No rule with id [{ruleId}] was found.");

        return await ExecuteRule(rule, executionContextId, request, cancellationToken);
    }

    private async Task TryPersistResults(T request, PolicyExecutionResult policyExecutionResult)
    {
        try
        {
            await resultsRepository.PersistResults(request, policyExecutionResult);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An exception occurred writing the results to the [{repositoryTypeName}] for [{ruleContextId}]",
                resultsRepository.GetType().Name,
                policyExecutionResult.RuleContextId);
        }
    }

    private async Task<IList<RuleExecutionResult>> Execute(
        Policy<T> policy,
        Guid executionContextId,
        T request,
        CancellationToken cancellationToken)
    {
        List<RuleExecutionResult> ruleExecutionResults = [];
        foreach (Rule<T> rule in policy.Rules)
        {
            RuleExecutionResult response = await ExecuteRule(rule, executionContextId, request, cancellationToken);
            ruleExecutionResults.Add(response);
        }

        return ruleExecutionResults;
    }

    private async Task<RuleExecutionResult> ExecuteRule(
        Rule<T> rule,
        Guid executionContextId,
        T request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("... executing [{policyId}]:[{policyName}] for [{executionContextId}]", rule.Id, rule.Name, executionContextId);

        RuleExecutionResult result = new(rule.Id, rule.Name, rule.Description);

        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            bool passed = await rule.Source.Invoke(request, cancellationToken);
            result.Passed = passed;
            if (!passed && rule.FailureMessage != null)
            {
                result.Message = rule.FailureMessage(request);
            }
        }
        catch (Exception ex)
        {
            result.Passed = false;
            result.Exception = ex;
            result.Message = ex.Message;
            logger.LogError(ex, "An exception occurred executing [{ruleId}]:[{ruleName}]", rule.Id, rule.Name);
        }
        finally
        {
            stopwatch.Stop();
            result.Elapsed = stopwatch.Elapsed;
            FlowRulesEventCounterSource.EventSource.RuleExecution(policy.Id, rule.Id, stopwatch.ElapsedMilliseconds);
        }

        return result;
    }
}
