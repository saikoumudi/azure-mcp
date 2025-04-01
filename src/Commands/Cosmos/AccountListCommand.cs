using AzureMCP.Arguments.Cosmos;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Cosmos;

public class AccountListCommand : BaseCosmosCommand<AccountListArguments>
{
    public AccountListCommand() : base()
    {
        // Register only required base arguments since no account-specific args needed
        RegisterArgumentChain();
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "list",
            "List all Cosmos DB accounts in a subscription. This command retrieves and displays all Cosmos DB accounts available in the specified subscription. Results include account names and are returned as a JSON array.");

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