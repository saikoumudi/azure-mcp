using System.CommandLine;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using AzureMCP.Arguments;
using AzureMCP.Arguments.Storage;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.CommandLine.Parsing;
using AzureMCP.Arguments.Storage.Blobs;

namespace AzureMCP.Commands.Storage;

public abstract class BaseStorageCommand<TArgs> : BaseCommand<TArgs> 
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

   
    // Override to provide the correct command path for examples
    protected override string GetCommandPath()
    {
        // Extract the command name from the class name (e.g., ContainersListCommand -> list)
        string commandName = GetType().Name.Replace("Command", "");
        
        // Get the full namespace path after Commands.Storage
        var nsPath = GetType().Namespace!.Split('.').SkipWhile(x => !x.Equals("Storage")).Skip(1);
        
        // Combine namespace path with command
        var parts = nsPath.Concat(new[] { commandName })
            .SelectMany(x => SplitCamelCase(x))
            .Select(x => x.ToLowerInvariant());

        // Return the full command path starting with "storage"
        return "storage " + string.Join(" ", parts);
    }

    private static IEnumerable<string> SplitCamelCase(string input)
    {
        return System.Text.RegularExpressions.Regex.Split(input, @"(?<!^)(?=[A-Z])");
    }

    // Common method to get storage account options
    protected async Task<List<ArgumentOption>> GetAccountOptions(CommandContext context, string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId)) return [];

        var storageService = context.GetService<IStorageService>();
        var accounts = await storageService.GetStorageAccounts(subscriptionId);
        
        return accounts?.Select(a => new ArgumentOption { Name = a, Id = a }).ToList() ?? [];
    }

    // Helper method to get container options
    protected async Task<List<ArgumentOption>> GetContainerOptions(CommandContext context, string accountName, string subscriptionId)
    {
        if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(subscriptionId))
        {
            return [];
        }

        var storageService = context.GetService<IStorageService>();
        var containers = await storageService.ListContainers(accountName, subscriptionId);
        
        return containers?.Select(c => new ArgumentOption { Name = c, Id = c }).ToList() 
            ?? [];
    }

    // Helper methods for creating Storage-specific arguments
    protected ArgumentChain<TArgs> CreateAccountArgument(Func<CommandContext, string, Task<List<ArgumentOption>>> accountOptionsLoader)
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Storage.Account.Name, ArgumentDefinitions.Storage.Account.Description)
            .WithCommandExample($"{GetCommandPath()} --account-name <account-name>")
            .WithValueAccessor(args => ((BaseStorageArguments)args).Account ?? string.Empty)
            .WithValueLoader(async (context, args) => await accountOptionsLoader(context, args.SubscriptionId ?? string.Empty))
            .WithIsRequired(true);
    }

    protected ArgumentChain<TArgs> CreateContainerArgument(
        Func<CommandContext, string, string, Task<List<ArgumentOption>>> containerOptionsLoader)
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Storage.Container.Name, ArgumentDefinitions.Storage.Container.Description)
            .WithCommandExample($"{GetCommandPath()} --container-name <container-name>")
            .WithValueAccessor(args => ((dynamic)args).Container ?? string.Empty)
            .WithValueLoader(async (context, args) => 
            {
                dynamic dynamicArgs = args;
                return await containerOptionsLoader(
                    context, 
                    dynamicArgs.Account ?? string.Empty, 
                    dynamicArgs.SubscriptionId ?? string.Empty);
            })
            .WithIsRequired(true);
    }

    protected ArgumentChain<TArgs> CreateTableArgument(
        Func<CommandContext, string, string, Task<List<ArgumentOption>>> tableOptionsLoader)
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Storage.Table.Name, ArgumentDefinitions.Storage.Table.Description)
            .WithCommandExample($"{GetCommandPath()} --table-name <table-name>")
            .WithValueAccessor(args => ((dynamic)args).Table ?? string.Empty)
            .WithValueLoader(async (context, args) => 
            {
                dynamic dynamicArgs = args;
                return await tableOptionsLoader(
                    context, 
                    dynamicArgs.Account ?? string.Empty, 
                    dynamicArgs.SubscriptionId ?? string.Empty);
            })
            .WithIsRequired(true);
    }
    protected override TArgs BindArguments(ParseResult parseResult)
    {
        // Just get base arguments, let derived classes handle their specific bindings
        return base.BindArguments(parseResult);
    }
}
