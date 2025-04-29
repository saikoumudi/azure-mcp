using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using AzureMcp.Commands;
using Microsoft.Extensions.Logging;
using AzureMcp.Arguments.Datadog.MonitoredResources;

namespace AzureMcp.Commands.Datadog.MonitoredResources;

public sealed class MonitoredResourcesListCommand(ILogger<MonitoredResourcesListCommand> logger) : SubscriptionCommand<MonitoredResourcesListArguments>()
{
    private readonly Option<string> _resourceGroupOption = new Option<string>("--resource-group", "The resource group name.");

    protected override string GetCommandName() => "list";

    protected override string GetCommandDescription() =>
        "Lists monitored resources in Datadog for a specific resource group.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_resourceGroupOption);
    }

    protected override MonitoredResourcesListArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption);
        return args;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            var service = context.GetService<IDatadogService>();
            var results = await service.ListMonitoredResources(args.ResourceGroup, args.Subscription!);

            context.Response.Results = results?.Count > 0 ? new { results } : null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    private void HandleException(CommandResponse response, Exception ex)
    {
        throw new NotImplementedException();
    }
}