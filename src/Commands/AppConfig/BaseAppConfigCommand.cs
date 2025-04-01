using AzureMCP.Arguments;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;

namespace AzureMCP.Commands.AppConfig;

public abstract class BaseAppConfigCommand<TArgs> : BaseCommand<TArgs> where TArgs : BaseArgumentsWithSubscription, new()
{
    protected readonly Option<string> _accountOption;

    protected BaseAppConfigCommand() : base()
    {
        _accountOption = ArgumentDefinitions.AppConfig.Account.ToOption();
    }

    // Common method to get account options
    protected async Task<List<ArgumentOption>> GetAccountOptions(CommandContext context, string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId)) return [];

        var appConfigService = context.GetService<IAppConfigService>();
        var accounts = await appConfigService.GetAppConfigAccounts(subscriptionId);

        return accounts?.Select(a => new ArgumentOption { Name = a.Name, Id = a.Name }).ToList() ?? [];
    }

    // Helper method for creating App Config-specific arguments
    protected ArgumentChain<TArgs> CreateAccountArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.AppConfig.Account.Name, ArgumentDefinitions.AppConfig.Account.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.AppConfig.Account))
            .WithValueAccessor(args => ((dynamic)args).Account ?? string.Empty)
            .WithValueLoader(async (context, args) =>
                await GetAccountOptions(context, args.Subscription ?? string.Empty))
            .WithIsRequired(ArgumentDefinitions.AppConfig.Account.Required);
    }

    // Helper method for creating key arguments
    protected ArgumentChain<TArgs> CreateKeyArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.AppConfig.Key.Name, ArgumentDefinitions.AppConfig.Key.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.AppConfig.Key))
            .WithValueAccessor(args => ((dynamic)args).Key ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.AppConfig.Key.Required);
    }

    // Helper method for creating value arguments
    protected ArgumentChain<TArgs> CreateValueArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.AppConfig.Value.Name, ArgumentDefinitions.AppConfig.Value.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.AppConfig.Value))
            .WithValueAccessor(args => ((dynamic)args).Value ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.AppConfig.Value.Required);
    }

    // Helper method for creating label arguments
    protected ArgumentChain<TArgs> CreateLabelArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.AppConfig.Label.Name, ArgumentDefinitions.AppConfig.Label.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.AppConfig.Label))
            .WithValueAccessor(args => ((dynamic)args).Label ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.AppConfig.Label.Required);
    }
}