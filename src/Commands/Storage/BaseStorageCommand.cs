using AzureMCP.Arguments.Storage;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Storage;

public abstract class BaseStorageCommand<TArgs> : BaseCommandWithSubscription<TArgs>
    where TArgs : BaseStorageArguments, new()
{
    protected readonly Option<string> _accountOption;
    protected readonly Option<string> _containerOption;
    protected readonly Option<string> _tableOption;

    protected BaseStorageCommand()
        : base()
    {
        _accountOption = ArgumentDefinitions.Storage.Account.ToOption();
        _containerOption = ArgumentDefinitions.Storage.Container.ToOption();
        _tableOption = ArgumentDefinitions.Storage.Table.ToOption();
    }

    // Common method to get storage account options
    protected async Task<List<ArgumentOption>> GetAccountOptions(CommandContext context, TArgs args)
    {
        if (string.IsNullOrEmpty(args.Subscription)) return [];

        var storageService = context.GetService<IStorageService>();
        var accounts = await storageService.GetStorageAccounts(args.Subscription);

        return accounts?.Select(a => new ArgumentOption { Name = a, Id = a }).ToList() ?? [];
    }

    // Helper method to get container options
    protected async Task<List<ArgumentOption>> GetContainerOptions(CommandContext context, TArgs args)
    {
        if (string.IsNullOrEmpty(args.Account) || string.IsNullOrEmpty(args.Subscription))
        {
            return [];
        }

        var storageService = context.GetService<IStorageService>();
        var containers = await storageService.ListContainers(args.Account, args.Subscription);

        return containers?.Select(c => new ArgumentOption { Name = c, Id = c }).ToList() ?? [];
    }

    // Helper methods for creating Storage-specific arguments
    protected ArgumentChain<TArgs> CreateAccountArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Storage.Account.Name, ArgumentDefinitions.Storage.Account.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Storage.Account))
            .WithValueAccessor(args => ((BaseStorageArguments)args).Account ?? string.Empty)
            .WithValueLoader(GetAccountOptions)
            .WithIsRequired(ArgumentDefinitions.Storage.Account.Required);
    }

    protected override TArgs BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        ((BaseStorageArguments)args).Account = parseResult.GetValueForOption(_accountOption);
        return args;
    }
}