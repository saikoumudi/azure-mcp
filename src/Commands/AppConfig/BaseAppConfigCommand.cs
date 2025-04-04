using AzureMCP.Arguments.AppConfig;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;

namespace AzureMCP.Commands.AppConfig;

public abstract class BaseAppConfigCommand<TArgs> : BaseCommandWithSubscription<TArgs> where TArgs : BaseAppConfigArguments, new()
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
            .WithValueAccessor(args => args.Account ?? string.Empty)
            .WithValueLoader(async (context, args) =>
                await GetAccountOptions(context, args.Subscription ?? string.Empty))
            .WithIsRequired(ArgumentDefinitions.AppConfig.Account.Required);
    }
}