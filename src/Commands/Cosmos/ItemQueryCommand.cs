// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Arguments.Cosmos;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Cosmos;

public sealed class ItemQueryCommand : BaseContainerCommand<ItemQueryArguments>
{
    private readonly Option<string> _queryOption = ArgumentDefinitions.Cosmos.Query.ToOption();

    protected override string GetCommandName() => "query";

    protected override string GetCommandDescription() =>
        $"""
        Execute a SQL query against items in a Cosmos DB container. Requires {ArgumentDefinitions.Cosmos.AccountName}, 
        {ArgumentDefinitions.Cosmos.DatabaseName}, and {ArgumentDefinitions.Cosmos.ContainerName}. 
        The {ArgumentDefinitions.Cosmos.QueryText} parameter accepts SQL query syntax. Results are returned as a 
        JSON array of documents.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_queryOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateQueryArgument());
    }

    private static ArgumentBuilder<ItemQueryArguments> CreateQueryArgument()
    {
        var defaultValue = ArgumentDefinitions.Cosmos.Query.DefaultValue ?? "SELECT * FROM c";
        return ArgumentBuilder<ItemQueryArguments>
            .Create(ArgumentDefinitions.Cosmos.Query.Name, ArgumentDefinitions.Cosmos.Query.Description)
            .WithValueAccessor(args => args.Query ?? string.Empty)
            .WithDefaultValue(defaultValue)
            .WithIsRequired(ArgumentDefinitions.Cosmos.Query.Required);
    }

    protected override ItemQueryArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Query = parseResult.GetValueForOption(_queryOption);
        return args;
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            if (!await ProcessArguments(context, args))
            {
                return context.Response;
            }

            var cosmosService = context.GetService<ICosmosService>();
            var items = await cosmosService.QueryItems(
                args.Account!,
                args.Database!,
                args.Container!,
                args.Query ?? "SELECT * FROM c",
                args.Subscription!,
                args.Tenant,
                args.RetryPolicy);

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