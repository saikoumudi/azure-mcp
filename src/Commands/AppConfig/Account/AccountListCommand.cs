using AzureMCP.Arguments.AppConfig.Account;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.AppConfig.Account;

public class AccountListCommand : BaseAppConfigCommand<AccountListArguments>
{
    public AccountListCommand() : base()
    {
        RegisterArgumentChain();
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "list",
            "List all App Configuration stores in a subscription. This command retrieves and displays all App Configuration stores available in the specified subscription. Results include store names returned as a JSON array.");

        AddBaseOptionsToCommand(command);
        return command;
    }

    protected override AccountListArguments BindArguments(ParseResult parseResult)
    {
        return base.BindArguments(parseResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindArguments(parseResult);

        try
        {
            if (!await ProcessArgumentChain(context, options))
            {
                return context.Response;
            }

            var appConfigService = context.GetService<IAppConfigService>();
            var accounts = await appConfigService.GetAppConfigAccounts(
                options.Subscription!,
                options.TenantId,
                options.RetryPolicy);

            context.Response.Results = accounts?.Count > 0 ?
                new { accounts } :
                null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}