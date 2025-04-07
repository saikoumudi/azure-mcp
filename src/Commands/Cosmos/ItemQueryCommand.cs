using AzureMCP.Arguments.Cosmos;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Cosmos;

public class ItemQueryCommand : BaseDatabaseCommand<ItemQueryArguments>
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

    private ArgumentChain<ItemQueryArguments> CreateContainerArgument()
    {
        return ArgumentChain<ItemQueryArguments>
            .Create(ArgumentDefinitions.Cosmos.Container.Name, ArgumentDefinitions.Cosmos.Container.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Cosmos.Container))
            .WithValueAccessor(args => args.Container ?? string.Empty)
            .WithValueLoader(async (context, args) =>
                await GetContainerOptions(
                    context,
                    args.Account ?? string.Empty,
                    args.Database ?? string.Empty,
                    args.Subscription ?? string.Empty))
            .WithIsRequired(ArgumentDefinitions.Cosmos.Container.Required);
    }

    protected ArgumentChain<ItemQueryArguments> CreateQueryArgument()
    {
        var defaultValue = ArgumentDefinitions.Cosmos.Query.DefaultValue ?? "SELECT * FROM c";
        return ArgumentChain<ItemQueryArguments>
            .Create(ArgumentDefinitions.Cosmos.Query.Name, ArgumentDefinitions.Cosmos.Query.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Cosmos.Query))
            .WithValueAccessor(args => args.Query ?? string.Empty)
            .WithDefaultValue(defaultValue)
            .WithIsRequired(ArgumentDefinitions.Cosmos.Query.Required);
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override Command GetCommand()
    {
        var command = new Command(
            "query",
            $"Execute a SQL query against items in a Cosmos DB container. Requires {ArgumentDefinitions.Cosmos.AccountName}, " +
            $"{ArgumentDefinitions.Cosmos.DatabaseName}, and {ArgumentDefinitions.Cosmos.ContainerName}. " +
            $"The {ArgumentDefinitions.Cosmos.QueryText} parameter accepts SQL query syntax. Results are returned as a JSON array of documents.");

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
                options.Subscription!,
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