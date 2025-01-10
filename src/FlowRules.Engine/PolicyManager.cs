using System;
using System.Collections.Generic;
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

        long startTime = TimeProvider.System.GetTimestamp();

        IList<RuleExecutionResult> response = await Execute(executionContextId, request, cancellationToken);

        PolicyExecutionResult policyExecutionResult =
            new()
            {
                RuleContextId = executionContextId,
                CorrelationId = correlationId,
                PolicyId = policy.Id,
                PolicyName = policy.Name,
                Version = policy.GetType().Assembly.GetName().Version?.ToString(4),
                RuleExecutionResults = [.. response],
                Passed = response.All(r => r.Passed)
            };

        await TryPersistResults(request, policyExecutionResult);

        FlowRulesEventCounterSource.EventSource.PolicyExecution(policy.Id, TimeProvider.System.GetElapsedTime(startTime));

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

        RuleExecutionResultBuilder ruleExecutionResultBuilder = new(rule.Id, rule.Name, rule.Description);

        long stampStart = TimeProvider.System.GetTimestamp();

        try
        {
            bool passed = await rule.Source.Invoke(request, cancellationToken);

            if (passed)
            {
                ruleExecutionResultBuilder.WithSuccess();
            }
            else
            {
                ruleExecutionResultBuilder.WithFailure(rule.FailureMessage?.Invoke(request));
            }
        }
        catch (Exception ex)
        {
            ruleExecutionResultBuilder.WithException(ex);
            logger.LogError(ex, "An exception occurred executing [{ruleId}]:[{ruleName}]", rule.Id, rule.Name);
        }
        finally
        {
            TimeSpan elapsed = TimeProvider.System.GetElapsedTime(stampStart);
            ruleExecutionResultBuilder.WithTime(elapsed);
            FlowRulesEventCounterSource.EventSource.RuleExecution(policy.Id, rule.Id, elapsed);
        }

        return ruleExecutionResultBuilder.ToRuleExecutionResult();
    }
}
