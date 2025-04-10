using AzureMCP.Arguments;
using AzureMCP.Arguments.Monitor;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Monitor;

public abstract class BaseMonitorCommand<TArgs>() : SubscriptionCommand<TArgs>
    where TArgs : SubscriptionArguments, IWorkspaceArguments, new()
{
    protected readonly Option<string> _workspaceOption = ArgumentDefinitions.Monitor.Workspace.ToOption();

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_workspaceOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateWorkspaceArgument());
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

    protected virtual ArgumentBuilder<TArgs> CreateWorkspaceArgument()
    {
        return ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Monitor.Workspace.Name, ArgumentDefinitions.Monitor.Workspace.Description)
            .WithValueAccessor(args => args.Workspace ?? string.Empty)
            .WithSuggestedValuesLoader(async (context, args) => await GetWorkspaceOptions(context, args.Subscription ?? string.Empty))
            .WithIsRequired(ArgumentDefinitions.Monitor.Workspace.Required);
    }

    protected override TArgs BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Workspace = parseResult.GetValueForOption(_workspaceOption);
        return args;
    }
}