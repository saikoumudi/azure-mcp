// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Arguments.Subscription;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Subscription;

public sealed class SubscriptionListCommand : GlobalCommand<SubscriptionListArguments>
{
    protected override string GetCommandName() => "list";

    protected override string GetCommandDescription() =>
        $"""
        List all Azure subscriptions accessible to your account. Optionally specify {ArgumentDefinitions.Common.TenantName}
        and {ArgumentDefinitions.Common.AuthMethodName}. Results include subscription names and IDs, returned as a JSON array.
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

            var subscriptionService = context.GetService<ISubscriptionService>();
            var subscriptions = await subscriptionService.GetSubscriptions(args.Tenant,
                args.RetryPolicy);

            context.Response.Results = subscriptions?.Count > 0 ? new { subscriptions } : null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}