using System;
using System.Threading;
using System.Threading.Tasks;

using FlowRules.Engine.Extensions;
using FlowRules.Engine.Interfaces;
using FlowRules.Engine.Models;
using FlowRules.Extensions.SqlServer;
using FlowRules.Samples.TestPolicy;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlowRules.Samples.Console;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args).Build();

        IConfiguration config = new ConfigurationBuilder()
            .AddUserSecrets(typeof(Program).Assembly)
            .AddEnvironmentVariables()
            .Build();

        ServiceCollection serviceCollection = GetServiceCollection(config);

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        IPolicyManager<MortgageApplication> policyManager = serviceProvider.GetService<IPolicyManager<MortgageApplication>>();

        MortgageApplication testMortgage = new(21, "FTB", 500_000, 70_000, 120_000, 1000, 2000, 25);

        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        PolicyExecutionResult results = await policyManager.Execute(Guid.NewGuid().ToString(), Guid.NewGuid(), testMortgage, cancellationToken);

        ILogger<MortgageApplication> logger = serviceProvider.GetService<ILogger<MortgageApplication>>();

        LogResults(results, logger);

        await host.StartAsync(cancellationToken);

        serviceProvider.Dispose();
    }

    private static ServiceCollection GetServiceCollection(IConfiguration config)
    {
        ServiceCollection serviceCollection = new();

        serviceCollection.AddLogging(opt => { opt.AddConsole(); });

#if SQLSERVER
        serviceCollection
            .AddOptions<SqlServerPolicyResultsRepositoryConfig>()
            .Bind(config.GetSection(nameof(SqlServerPolicyResultsRepositoryConfig)));
#endif

        serviceCollection.AddFlowRules<MortgageApplication>(() => PolicySetup.GetPolicy(), c =>
        {
#if SQLSERVER
            c.ResultsRepository = typeof(SqlServerPolicyResultsRepository<MortgageApplication>);
#endif
        });
        return serviceCollection;
    }

    private static void LogResults(PolicyExecutionResult results, ILogger<MortgageApplication> logger)
    {
        logger.LogInformation("[{RuleContextId}] [{PolicyId}]:[{PolicyName}:{Version}] - {Passed}",
            results.RuleContextId,
            results.PolicyId,
            results.PolicyName,
            results.Version,
            results.Passed);

        if (results.RuleExecutionResults.Length > 0)
        {
            foreach (RuleExecutionResult result in results.RuleExecutionResults)
            {
                logger.LogInformation("[{Id}]:[{Name}] - {Passed} {Message} ({Elapsed}ms)", result.Id, result.Name, result.Passed, result.Message ?? string.Empty, result.Elapsed);
            }
        }
    }
}
