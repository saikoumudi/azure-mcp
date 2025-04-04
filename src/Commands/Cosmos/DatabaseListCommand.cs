using AzureMCP.Arguments.Cosmos;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Cosmos;

public class DatabaseListCommand : BaseCosmosCommand<DatabaseListArguments>
{
    public DatabaseListCommand() : base()
    {
        RegisterArgumentChain(
            CreateAccountArgument()
        );
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override Command GetCommand()
    {
        var command = new Command(
            "list",
            "List all databases in a Cosmos DB account. This command retrieves and displays all databases available in the specified Cosmos DB account. Results include database names and are returned as a JSON array.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_accountOption);
        return command;
    }

    protected override DatabaseListArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Account = parseResult.GetValueForOption(_accountOption);
        return args;
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
            var databases = await cosmosService.ListDatabases(
                options.Account!,
                options.Subscription!,
                options.TenantId,
                options.RetryPolicy);

            context.Response.Results = databases?.Count > 0 ?
                new { databases } :
                null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}