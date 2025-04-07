using AzureMCP.Arguments;
using AzureMCP.Arguments.Monitor;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using System.CommandLine;

namespace AzureMCP.Commands.Monitor;

public abstract class BaseMonitorCommand<TArgs> : BaseCommandWithSubscription<TArgs> where TArgs : BaseArgumentsWithSubscription, IWorkspaceArguments, new()
{
    protected readonly Option<string> _workspaceOption;

    protected BaseMonitorCommand()
        : base()
    {
        _workspaceOption = ArgumentDefinitions.Monitor.Workspace.ToOption();
    }

    protected async Task<List<ArgumentOption>> GetWorkspaceOptions(CommandContext context, string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId)) return [];

        var monitorService = context.GetService<IMonitorService>();
        var workspaces = await monitorService.ListWorkspaces(subscriptionId, null);

        return [.. workspaces.Select(w => new ArgumentOption
        {
            Name = w.Name,
            Id = w.CustomerId?.ToString() ?? string.Empty
        })];
    }

    protected virtual ArgumentChain<TArgs> CreateWorkspaceArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Monitor.Workspace.Name, ArgumentDefinitions.Monitor.Workspace.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Monitor.Workspace))
            .WithValueAccessor(args => args.Workspace ?? string.Empty)
            .WithValueLoader(async (context, args) => await GetWorkspaceOptions(context, args.Subscription ?? string.Empty))
            .WithIsRequired(ArgumentDefinitions.Monitor.Workspace.Required);
    }
}