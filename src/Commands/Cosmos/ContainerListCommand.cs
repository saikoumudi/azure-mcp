// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Arguments.Cosmos;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Cosmos;

public sealed class ContainerListCommand : BaseDatabaseCommand<ContainerListArguments>
{
    protected override string GetCommandName() => "list";

    protected override string GetCommandDescription() =>
        """
        List all containers in a Cosmos DB database. This command retrieves and displays all containers within 
        the specified database and Cosmos DB account. Results include container names and are returned as a 
        JSON array. You must specify both an account name and a database name.
        """;




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
            var containers = await cosmosService.ListContainers(
                args.Account!,
                args.Database!,
                args.Subscription!,
                args.Tenant,
                args.RetryPolicy);

            context.Response.Results = containers?.Count > 0 ?
                new { containers } :
                null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}