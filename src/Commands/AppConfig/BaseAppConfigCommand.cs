using AzureMCP.Arguments.AppConfig;
using AzureMCP.Models.Argument;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.AppConfig;

public abstract class BaseAppConfigCommand<T> : SubscriptionCommand<T> where T : BaseAppConfigArguments, new()
{
    protected readonly Option<string> _accountOption = ArgumentDefinitions.AppConfig.Account.ToOption();

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_accountOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateAccountArgument());
    }

    protected override T BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Account = parseResult.GetValueForOption(_accountOption);
        return args;
    }

    protected ArgumentBuilder<T> CreateAccountArgument()
    {
        return ArgumentBuilder<T>
            .Create(ArgumentDefinitions.AppConfig.Account.Name, ArgumentDefinitions.AppConfig.Account.Description)
            .WithValueAccessor(args => args.Account ?? string.Empty)
            .WithSuggestedValuesLoader(async (context, args) =>
            {
                if (string.IsNullOrEmpty(args.Subscription)) return [];

                var appConfigService = context.GetService<IAppConfigService>();
                var accounts = await appConfigService.GetAppConfigAccounts(args.Subscription);

                return accounts?.Select(a => new ArgumentOption { Name = a.Name, Id = a.Name }).ToList() ?? [];
            })
            .WithIsRequired(ArgumentDefinitions.AppConfig.Account.Required);
    }
}