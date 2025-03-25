using AzureMCP.Arguments;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using Microsoft.Azure.Cosmos;
using System.CommandLine;

namespace AzureMCP.Commands.Cosmos;

public abstract class BaseCosmosCommand<TArgs> : BaseCommand<TArgs> where TArgs : BaseArgumentsWithSubscriptionId, new()
{
    protected readonly Option<string> _accountOption;
    protected readonly Option<string> _databaseOption;
    protected readonly Option<string> _containerOption;

    protected BaseCosmosCommand() : base()
    {
        _accountOption = ArgumentDefinitions.Cosmos.Account.ToOption();
        _databaseOption = ArgumentDefinitions.Cosmos.Database.ToOption();
        _containerOption = ArgumentDefinitions.Cosmos.Container.ToOption();
    }

    // Common method to get account options
    protected async Task<List<ArgumentOption>> GetAccountOptions(CommandContext context, string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId)) return [];

        var cosmosService = context.GetService<ICosmosService>();
        var accounts = await cosmosService.GetCosmosAccounts(subscriptionId);

        return accounts?.Select(a => new ArgumentOption { Name = a, Id = a }).ToList() ?? [];
    }

    // Common method to get database options
    protected async Task<List<ArgumentOption>> GetDatabaseOptions(CommandContext context, string accountName, string subscriptionId)
    {
        if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(subscriptionId))
            return [];

        var cosmosService = context.GetService<ICosmosService>();
        var databases = await cosmosService.ListDatabases(accountName, subscriptionId);

        return databases?.Select(d => new ArgumentOption { Name = d, Id = d }).ToList() ?? [];
    }

    // Common method to get container options
    protected async Task<List<ArgumentOption>> GetContainerOptions(CommandContext context, string accountName, string databaseName, string subscriptionId)
    {
        if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(subscriptionId))
            return [];

        var cosmosService = context.GetService<ICosmosService>();
        var containers = await cosmosService.ListContainers(accountName, databaseName, subscriptionId);

        return containers?.Select(c => new ArgumentOption { Name = c, Id = c }).ToList() ?? [];
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        CosmosException cosmosEx => cosmosEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        CosmosException cosmosEx => (int)cosmosEx.StatusCode,
        _ => base.GetStatusCode(ex)
    };

    // Helper methods for creating Cosmos-specific arguments
    protected ArgumentChain<TArgs> CreateAccountArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Cosmos.Account.Name, ArgumentDefinitions.Cosmos.Account.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Cosmos.Account))
            .WithValueAccessor(args => ((dynamic)args).Account ?? string.Empty)
            .WithValueLoader(async (context, args) =>
                await GetAccountOptions(context, args.SubscriptionId ?? string.Empty))
            .WithIsRequired(ArgumentDefinitions.Cosmos.Account.Required);
    }

    protected ArgumentChain<TArgs> CreateDatabaseArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Cosmos.Database.Name, ArgumentDefinitions.Cosmos.Database.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Cosmos.Database))
            .WithValueAccessor(args => ((dynamic)args).Database ?? string.Empty)
            .WithValueLoader(async (context, args) =>
                await GetDatabaseOptions(context, ((dynamic)args).Account ?? string.Empty, args.SubscriptionId ?? string.Empty))
            .WithIsRequired(ArgumentDefinitions.Cosmos.Database.Required);
    }

    protected ArgumentChain<TArgs> CreateContainerArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Cosmos.Container.Name, ArgumentDefinitions.Cosmos.Container.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Cosmos.Container))
            .WithValueAccessor(args => ((dynamic)args).Container ?? string.Empty)
            .WithValueLoader(async (context, args) =>
                await GetContainerOptions(
                    context,
                    ((dynamic)args).Account ?? string.Empty,
                    ((dynamic)args).Database ?? string.Empty,
                    args.SubscriptionId ?? string.Empty))
            .WithIsRequired(ArgumentDefinitions.Cosmos.Container.Required);
    }

    protected ArgumentChain<TArgs> CreateQueryArgument()
    {
        var defaultValue = ArgumentDefinitions.Cosmos.Query.DefaultValue ?? "SELECT * FROM c";
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Cosmos.Query.Name, ArgumentDefinitions.Cosmos.Query.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Cosmos.Query))
            .WithValueAccessor(args => ((dynamic)args).Query ?? string.Empty)
            .WithDefaultValue(defaultValue)
            .WithIsRequired(ArgumentDefinitions.Cosmos.Query.Required);
    }
}