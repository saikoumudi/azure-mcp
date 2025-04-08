using AzureMCP.Arguments.Monitor;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Monitor.Workspace;

public class WorkspaceListCommand : BaseCommandWithSubscription<WorkspaceListArguments>
{
    public WorkspaceListCommand() : base()
    {
        RegisterArgumentChain();
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
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
        var args = BindArguments(parseResult);

        try
        {
            if (!await ProcessArgumentChain(context, args))
            {
                return context.Response;
            }

            var monitorService = context.GetService<IMonitorService>();
            var workspaces = await monitorService.ListWorkspaces(
                args.Subscription!,
                args.Tenant,
                args.RetryPolicy);

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