using AzureMCP.Arguments.Group;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Group;

public class GroupListCommand : BaseCommandWithSubscription<BaseGroupArguments>
{
    public GroupListCommand() : base()
    {
        RegisterArgumentChain();
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override Command GetCommand()
    {
        var command = new Command(
            "list",
            $"List all resource groups in a subscription. This command retrieves all resource groups available " +
            $"in the specified {ArgumentDefinitions.Common.SubscriptionName}. Results include resource group names and IDs, " +
            "returned as a JSON array.");

        AddBaseOptionsToCommand(command);
        return command;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            if (!await ProcessArgumentChain(context, args))
            {
                return context.Response;
            }

            var resourceGroupService = context.GetService<IResourceGroupService>();
            var groups = await resourceGroupService.GetResourceGroups(
                args.Subscription!,
                args.Tenant,
                args.RetryPolicy);

            context.Response.Results = groups?.Count > 0 ?
                new { groups } :
                null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}