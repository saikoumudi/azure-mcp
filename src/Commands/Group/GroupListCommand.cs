using AzureMCP.Arguments;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Group;

public class GroupListCommand : BaseCommandWithSubscription<BaseArgumentsWithSubscription>
{
    public GroupListCommand() : base()
    {
        RegisterArgumentChain();
    }

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
        var options = BindArguments(parseResult);

        try
        {
            if (!await ProcessArgumentChain(context, options))
            {
                return context.Response;
            }

            var resourceGroupService = context.GetService<IResourceGroupService>();
            var groups = await resourceGroupService.GetResourceGroups(
                options.Subscription!,
                options.TenantId,
                options.RetryPolicy);

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
