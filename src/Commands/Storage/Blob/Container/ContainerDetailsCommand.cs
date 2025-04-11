// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Arguments.Storage.Blob.Container;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine.Parsing;

namespace AzureMcp.Commands.Storage.Blob.Container;

public sealed class ContainerDetailsCommand : BaseContainerCommand<ContainerDetailsArguments>
{
    protected override string GetCommandName() => "details";

    protected override string GetCommandDescription() =>
        $"""
        Get detailed properties of a storage container including metadata, lease status, and access level.
        Requires {ArgumentDefinitions.Storage.AccountName} and {ArgumentDefinitions.Storage.ContainerName}.
        """;

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var args = BindArguments(parseResult);

            if (!await ProcessArguments(context, args))
            {
                return context.Response;
            }

            var storageService = context.GetService<IStorageService>();
            var details = await storageService.GetContainerDetails(
                args.Account!,
                args.Container!,
                args.Subscription!,
                args.Tenant,
                args.RetryPolicy
            );

            context.Response.Results = new { details };
            return context.Response;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
            return context.Response;
        }
    }
}