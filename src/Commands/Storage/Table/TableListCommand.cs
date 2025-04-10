// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Arguments.Storage.Table;
using AzureMCP.Models;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Storage.Table;

public sealed class TableListCommand : BaseStorageCommand<TableListArguments>
{
    protected override string GetCommandName() => "list";

    protected override string GetCommandDescription() =>
        """
        List all tables in a Storage account. This command retrieves and displays all tables available in the specified Storage account.
        Results include table names and are returned as a JSON array. You must specify an account name and subscription ID.
        Use this command to explore your Storage resources or to verify table existence before performing operations on specific tables.
        """;

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult commandOptions)
    {
        var args = BindArguments(commandOptions);

        try
        {
            if (!await ProcessArguments(context, args))
            {
                return context.Response;
            }

            var storageService = context.GetService<IStorageService>();
            var tables = await storageService.ListTables(
                args.Account!,
                args.Subscription!,
                args.AuthMethod ?? AuthMethod.Credential,
                null,
                args.Tenant,
                args.RetryPolicy);

            context.Response.Results = tables?.Count > 0 ? new { tables } : null;

            // Only show warning if we actually had to fall back to a different auth method
            if (context.Response.Results is not null && !string.IsNullOrEmpty(context.Response.Message))
            {
                var authMethod = args.AuthMethod ?? AuthMethod.Credential;
                context.Response.Message = authMethod switch
                {
                    AuthMethod.Credential when context.Response.Message.Contains("connection string") =>
                        "Warning: Credential and key auth failed, succeeded using connection string. " +
                        "Consider using --auth-method connectionString for future calls.",
                    AuthMethod.Key when context.Response.Message.Contains("connection string") =>
                        "Warning: Key auth failed, succeeded using connection string. " +
                        "Consider using --auth-method connectionString for future calls.",
                    _ => string.Empty // Clear any warning message if auth succeeded directly
                };
            }
            else
            {
                context.Response.Message = string.Empty;
            }
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}