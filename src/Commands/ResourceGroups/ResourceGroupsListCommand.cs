using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMCP.Arguments;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;

namespace AzureMCP.Commands.ResourceGroups;

public class ResourceGroupsListCommand : BaseCommand<BaseArgumentsWithSubscriptionId>
{
    public ResourceGroupsListCommand() : base()
    {
        RegisterArgumentChain();
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "list",
            "List resource groups in a subscription. This command retrieves all resource groups available in the specified Azure subscription, displaying their names, IDs, and locations. Use this command to identify target resource groups for other operations.");

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
            var resourceGroups = await resourceGroupService.GetResourceGroups(
                options.SubscriptionId!,
                options.TenantId,
                options.RetryPolicy);

            context.Response.Results = resourceGroups?.Count > 0 ? 
                new { resourceGroups } : 
                null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}
