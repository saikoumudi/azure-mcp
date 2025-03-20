using System.CommandLine;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using AzureMCP.Arguments.Cosmos;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Cosmos;

public class AccountsListCommand : BaseCosmosCommand<AccountsListArguments>
{
    public AccountsListCommand() : base()
    {
        // Register only required base arguments since no account-specific args needed
        RegisterArgumentChain();
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "list",
            "List all Cosmos DB accounts in a subscription. This command retrieves and displays all Cosmos DB accounts available in the specified subscription. Results include account names and are returned as a JSON array. You must specify a subscription ID.");

        AddBaseOptionsToCommand(command);
        return command;
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

            var cosmosService = context.GetService<ICosmosService>();
            var accounts = await cosmosService.GetCosmosAccounts(
                options.SubscriptionId!,
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