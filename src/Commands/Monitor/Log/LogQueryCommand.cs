using AzureMCP.Arguments.Monitor;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Monitor.Log;

public class LogQueryCommand : BaseMonitorCommand<LogQueryArguments>
{
    protected Option<string> _tableOption;
    protected Option<string> _queryOption;
    protected Option<int> _hoursOption;
    protected Option<int> _limitOption;

    public LogQueryCommand() : base()
    {
        _tableOption = new Option<string>(
           "--table",
           "The name of the Log Analytics table to query");

        _queryOption = new Option<string>(
            $"--{ArgumentDefinitions.Monitor.QueryTextName}",
            ArgumentDefinitions.Monitor.Query.Description);

        _hoursOption = new Option<int>(
            $"--{ArgumentDefinitions.Monitor.HoursName}",
            () => ArgumentDefinitions.Monitor.Hours.DefaultValue,
            ArgumentDefinitions.Monitor.Hours.Description);

        _limitOption = new Option<int>(
            $"--{ArgumentDefinitions.Monitor.LimitName}",
            () => ArgumentDefinitions.Monitor.Limit.DefaultValue,
            ArgumentDefinitions.Monitor.Limit.Description);

        RegisterArgumentChain(
            CreateWorkspaceIdArgument(GetWorkspaceOptions),
            CreateTableArgument(),
            CreateQueryArgument(),
            CreateHoursArgument(),
            CreateLimitArgument()
        );
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "query",
            "Query logs from Azure Monitor using KQL. This command allows you to execute Kusto Query Language (KQL) queries against your Log Analytics workspace to retrieve and analyze log data. You can specify the time range and limit the number of results returned.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_workspaceIdOption);
        command.AddOption(_tableOption);
        command.AddOption(_queryOption);
        command.AddOption(_hoursOption);
        command.AddOption(_limitOption);

        return command;
    }


    protected ArgumentChain<LogQueryArguments> CreateTableArgument()
    {
        return ArgumentChain<LogQueryArguments>
            .Create("table", "The name of the Log Analytics table to query")
            .WithCommandExample($"{GetCommandPath()} --table \"MyTable\"")
            .WithValueAccessor(args => args.Table ?? string.Empty)
            .WithIsRequired(true);
    }

    protected ArgumentChain<LogQueryArguments> CreateQueryArgument()
    {
        return ArgumentChain<LogQueryArguments>
            .Create(
                ArgumentDefinitions.Monitor.QueryTextName,
                ArgumentDefinitions.Monitor.Query.Description)
            .WithCommandExample($"{GetCommandPath()} --{ArgumentDefinitions.Monitor.QueryTextName} \"<kql-query>\"")
            .WithValueAccessor(args => args.Query ?? string.Empty)
            .WithIsRequired(true);
    }

    protected ArgumentChain<LogQueryArguments> CreateHoursArgument()
    {
        return ArgumentChain<LogQueryArguments>
            .Create(
                ArgumentDefinitions.Monitor.HoursName,
                ArgumentDefinitions.Monitor.Hours.Description)
            .WithCommandExample($"{GetCommandPath()} --{ArgumentDefinitions.Monitor.HoursName} {ArgumentDefinitions.Monitor.Hours.DefaultValue}")
            .WithValueAccessor(args => args.Hours?.ToString() ?? ArgumentDefinitions.Monitor.Hours.DefaultValue.ToString())
            .WithDefaultValue(ArgumentDefinitions.Monitor.Hours.DefaultValue.ToString())
            .WithIsRequired(false);
    }

    protected ArgumentChain<LogQueryArguments> CreateLimitArgument()
    {
        return ArgumentChain<LogQueryArguments>
            .Create(
                ArgumentDefinitions.Monitor.LimitName,
                ArgumentDefinitions.Monitor.Limit.Description)
            .WithCommandExample($"{GetCommandPath()} --{ArgumentDefinitions.Monitor.LimitName} {ArgumentDefinitions.Monitor.Limit.DefaultValue}")
            .WithValueAccessor(args => args.Limit?.ToString() ?? ArgumentDefinitions.Monitor.Limit.DefaultValue.ToString())
            .WithDefaultValue(ArgumentDefinitions.Monitor.Limit.DefaultValue.ToString())
            .WithIsRequired(false);
    }


    protected override LogQueryArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.WorkspaceId = parseResult.GetValueForOption(_workspaceIdOption);
        args.Table = parseResult.GetValueForOption(_tableOption);
        args.Query = parseResult.GetValueForOption(_queryOption);
        args.Hours = parseResult.GetValueForOption(_hoursOption);
        args.Limit = parseResult.GetValueForOption(_limitOption);
        return args;
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
            var results = await monitorService.QueryLogs(
                options.WorkspaceId!,
                options.Query!,
                options.Table!,
                options.Hours,
                options.Limit,
                options.SubscriptionId!,
                options.TenantId,
                options.RetryPolicy);

            context.Response.Results = results;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}