using AzureMCP.Arguments.Cosmos;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Cosmos;

public class ItemQueryCommand : BaseCosmosCommand<ItemQueryArguments>
{
    public ItemQueryCommand() : base()
    {
        RegisterArgumentChain(
            CreateAccountArgument(),
            CreateDatabaseArgument(),
            CreateContainerArgument(),
            CreateQueryArgument()
        );
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "query",
            "Execute a SQL query against items in a Cosmos DB container. This command allows you to retrieve documents from a container using SQL query syntax. Results are returned as a JSON array of documents.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_accountOption);
        command.AddOption(_databaseOption);
        command.AddOption(_containerOption);
        command.AddOption(ArgumentDefinitions.Cosmos.Query.ToOption());
        return command;
    }

    protected override ItemQueryArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Account = parseResult.GetValueForOption(_accountOption);
        args.Database = parseResult.GetValueForOption(_databaseOption);
        args.Container = parseResult.GetValueForOption(_containerOption);
        args.Query = parseResult.GetValueForOption(ArgumentDefinitions.Cosmos.Query.ToOption());
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
            var items = await cosmosService.QueryItems(
                options.Account!,
                options.Database!,
                options.Container!,
                options.Query ?? "SELECT * FROM c",
                options.SubscriptionId!,
                options.TenantId,
                options.RetryPolicy);

            context.Response.Results = items?.Count > 0 ?
                new { items } :
                null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}
