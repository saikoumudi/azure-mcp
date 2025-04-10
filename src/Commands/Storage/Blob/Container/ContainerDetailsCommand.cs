// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Arguments.Storage.Blob.Container;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Storage.Blob.Container;

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