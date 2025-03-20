using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using AzureMCP.Arguments.Monitor;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;

namespace AzureMCP.Commands.Monitor.Workspaces;

public class WorkspacesListCommand : BaseCommand<WorkspacesListArguments>
{
    public WorkspacesListCommand() : base()
    {
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "list", 
            "List Log Analytics workspaces in a subscription. This command retrieves all Log Analytics workspaces available in the specified Azure subscription, displaying their names, IDs, and other key properties. Use this command to identify workspaces before querying their logs or tables.");
            
        
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

            var monitorService = context.GetService<IMonitorService>();
            var workspaces = await monitorService.ListWorkspaces(
                options.SubscriptionId!,
                options.TenantId,
                options.RetryPolicy);

            context.Response.Results = workspaces?.Count > 0 ? 
                new { workspaces } : 
                null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}
