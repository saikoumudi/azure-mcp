using AzureMCP.Arguments;
using AzureMCP.Arguments.Monitor;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Monitor.Log;

public class LogQueryCommand : BaseMonitorCommand<LogQueryArguments>
{
    protected readonly Option<string> _tableNameOption;
    protected readonly Option<string> _queryOption;
    protected readonly Option<int> _hoursOption;
    protected readonly Option<int> _limitOption;

    public LogQueryCommand() : base()
    {
        _tableNameOption = ArgumentDefinitions.Monitor.TableName.ToOption();
        _queryOption = ArgumentDefinitions.Monitor.Query.ToOption();
        _hoursOption = ArgumentDefinitions.Monitor.Hours.ToOption();
        _limitOption = ArgumentDefinitions.Monitor.Limit.ToOption();

        RegisterArgumentChain(
            CreateWorkspaceArgument(),
            CreateResourceGroupArgument()!,
            CreateTableNameArgument(),
            CreateQueryArgument(),
            CreateHoursArgument(),
            CreateLimitArgument()
        );
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override Command GetCommand()
    {
        var command = new Command(
            "query",
            $"Execute a KQL query against a Log Analytics workspace. Requires {ArgumentDefinitions.Monitor.WorkspaceIdOrName} and resource group. " +
            $"Optional {ArgumentDefinitions.Monitor.HoursName} (default: {ArgumentDefinitions.Monitor.Hours.DefaultValue}) " +
            $"and {ArgumentDefinitions.Monitor.LimitName} (default: {ArgumentDefinitions.Monitor.Limit.DefaultValue}) parameters. " +
            $"The {ArgumentDefinitions.Monitor.QueryTextName} parameter accepts KQL syntax.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_workspaceOption);
        command.AddOption(_resourceGroupOption);
        command.AddOption(_tableNameOption);
        command.AddOption(_queryOption);
        command.AddOption(_hoursOption);
        command.AddOption(_limitOption);
        return command;
    }

    protected static async Task<List<ArgumentOption>> GetTableNameOptions(CommandContext context, string subscriptionId, string resourceGroup, string workspace, string? tableType = "CustomLog", string? tenant = null, RetryPolicyArguments? retryPolicy = null)
    {
        if (string.IsNullOrEmpty(subscriptionId) || string.IsNullOrEmpty(resourceGroup) || string.IsNullOrEmpty(workspace))
            return [];

        var monitorService = context.GetService<IMonitorService>();
        var tables = await monitorService.ListTables(subscriptionId, resourceGroup, workspace, tableType, tenant, retryPolicy);

        return [.. tables.Select(t => new ArgumentOption
        {
            Name = t,
            Id = t
        })];
    }

    protected virtual ArgumentChain<LogQueryArguments> CreateTableNameArgument()
    {
        return ArgumentChain<LogQueryArguments>
            .Create(ArgumentDefinitions.Monitor.TableName.Name, ArgumentDefinitions.Monitor.TableName.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Monitor.TableName))
            .WithValueAccessor(args =>
            {
                try
                {
                    return args.TableName ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            })
            .WithIsRequired(ArgumentDefinitions.Monitor.TableName.Required);
    }

    protected ArgumentChain<LogQueryArguments> CreateQueryArgument()
    {
        return ArgumentChain<LogQueryArguments>
            .Create(ArgumentDefinitions.Monitor.Query.Name, ArgumentDefinitions.Monitor.Query.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Monitor.Query))
            .WithValueAccessor(args => args.Query ?? string.Empty)
            .WithIsRequired(true);
    }

    protected ArgumentChain<LogQueryArguments> CreateHoursArgument()
    {
        return ArgumentChain<LogQueryArguments>
            .Create(ArgumentDefinitions.Monitor.Hours.Name, ArgumentDefinitions.Monitor.Hours.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Monitor.Hours))
            .WithValueAccessor(args => args.Hours?.ToString() ?? ArgumentDefinitions.Monitor.Hours.DefaultValue.ToString())
            .WithDefaultValue(ArgumentDefinitions.Monitor.Hours.DefaultValue.ToString())
            .WithIsRequired(false);
    }

    protected ArgumentChain<LogQueryArguments> CreateLimitArgument()
    {
        return ArgumentChain<LogQueryArguments>
            .Create(ArgumentDefinitions.Monitor.Limit.Name, ArgumentDefinitions.Monitor.Limit.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Monitor.Limit))
            .WithValueAccessor(args => args.Limit?.ToString() ?? ArgumentDefinitions.Monitor.Limit.DefaultValue.ToString())
            .WithDefaultValue(ArgumentDefinitions.Monitor.Limit.DefaultValue.ToString())
            .WithIsRequired(false);
    }

    protected override LogQueryArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Workspace = parseResult.GetValueForOption(_workspaceOption);
        args.ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption);
        args.TableName = parseResult.GetValueForOption(_tableNameOption);
        args.Query = parseResult.GetValueForOption(_queryOption);
        args.Hours = parseResult.GetValueForOption(_hoursOption);
        args.Limit = parseResult.GetValueForOption(_limitOption);
        return args;
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
            var results = await monitorService.QueryLogs(
                args.Subscription!,
                args.Workspace!,
                args.Query!,
                args.TableName!,
                args.Hours,
                args.Limit,
                args.Tenant,
                args.RetryPolicy);

            context.Response.Results = results;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}