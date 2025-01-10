using System.Data;
using System.Text.Json;
using System.Transactions;
using Dapper;
using FlowRules.Engine.Interfaces;
using FlowRules.Engine.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FlowRules.Extensions.SqlServer;

public class SqlServerPolicyResultsRepository<T> : IPolicyResultsRepository<T>
    where T : class
{
    private readonly SqlServerPolicyResultsRepositoryConfig _config;

    private readonly string _sqlInsertFlowRulesRequest;
    private readonly string _sqlInsertFlowRulesPolicyResult;
    private readonly string _sqlInsertFlowRulesRuleResult;

    public SqlServerPolicyResultsRepository(IOptions<SqlServerPolicyResultsRepositoryConfig> config)
    {
        _config = config.Value;

        if (string.IsNullOrEmpty(_config.ConnectionString))
        {
            throw new InvalidOperationException($"[{nameof(_config.ConnectionString)}] is not set.");
        }

        if (string.IsNullOrEmpty(_config.SchemaName))
        {
            throw new InvalidOperationException($"[{nameof(_config.SchemaName)}] is not set.");
        }
            
        _sqlInsertFlowRulesRequest = @$"
                INSERT INTO [{_config.SchemaName}].[FlowRulesRequest]
                    (FlowExecutionId, CorrelationId, PolicyId, Request)
                VALUES
                    (@FlowExecutionId, @CorrelationId, @PolicyId, @Request)

                SELECT SCOPE_IDENTITY()
            ";

        _sqlInsertFlowRulesPolicyResult = $@"
                INSERT INTO [{_config.SchemaName}].[FlowRulesPolicyResult]
                    (FlowRulesRequest_Id, PolicyName, Passed, Version)  
                VALUES 
                    (@FlowRulesRequest_Id, @PolicyName, @Passed, @Version)  

                SELECT SCOPE_IDENTITY()
            ";

        _sqlInsertFlowRulesRuleResult = $@"
                INSERT INTO [{_config.SchemaName}].[FlowRulesRuleResult]
                    (FlowRulesPolicyResult_Id, RuleId, RuleName, RuleDescription, Passed, Message, Elapsed, Exception)
                VALUES
                    (@FlowRulesPolicyResult_Id, @RuleId, @RuleName, @RuleDescription, @Passed, @Message, @Elapsed, @Exception)
            ";
    }

    public async Task PersistResults(T request, PolicyExecutionResult policyExecutionResult)
    {
        using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

        await using SqlConnection connection = new(_config.ConnectionString);

        connection.Open();

        int requestId = await connection.ExecuteScalarAsync<int>(_sqlInsertFlowRulesRequest, new
        {
            FlowExecutionId = policyExecutionResult.RuleContextId,
            CorrelationId = policyExecutionResult.CorrelationId,
            PolicyId = policyExecutionResult.PolicyId,
            Request = JsonSerializer.Serialize(request)
        });

        int policyResultId = await connection.ExecuteScalarAsync<int>(_sqlInsertFlowRulesPolicyResult, new
        {
            FlowRulesRequest_Id = requestId,
            PolicyName = policyExecutionResult.PolicyName,
            Passed = policyExecutionResult.Passed,
            Version = policyExecutionResult.Version
        });

        foreach (RuleExecutionResult? ruleResult in policyExecutionResult.RuleExecutionResults)
        {
            await connection.ExecuteAsync(_sqlInsertFlowRulesRuleResult, new
            {
                FlowRulesPolicyResult_Id = policyResultId,
                RuleId = ruleResult.Id,
                RuleName = ruleResult.Name,
                RuleDescription = ruleResult.Description,
                Passed = ruleResult.Passed,
                Message = ruleResult.Message,
                Elapsed = ruleResult.Elapsed,
                Exception = ruleResult.Exception?.Message
            });
        }

        scope.Complete();
    }
}
