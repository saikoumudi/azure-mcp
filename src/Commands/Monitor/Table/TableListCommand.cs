using AzureMCP.Arguments.Monitor;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Monitor.Table;

public class TableListCommand : BaseMonitorCommand<TableListArguments>
{
    private readonly Option<string> _tableTypeOption;

    public TableListCommand() : base()
    {
        _tableTypeOption = ArgumentDefinitions.Monitor.TableType.ToOption();

        RegisterArgumentChain(
           CreateWorkspaceArgument(),
           CreateResourceGroupArgument()!,
           CreateTableTypeArgument()
       );
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override Command GetCommand()
    {
        var command = new Command(
            "list",
            $"List all tables in a Log Analytics workspace. Requires {ArgumentDefinitions.Monitor.WorkspaceIdOrName}. " +
            "Returns table names and schemas that can be used for constructing KQL queries.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_workspaceOption);
        command.AddOption(_resourceGroupOption);
        command.AddOption(_tableTypeOption);
        return command;
    }

    protected override TableListArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Workspace = parseResult.GetValueForOption(_workspaceOption);
        args.ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption);
        args.TableType = parseResult.GetValueForOption(_tableTypeOption);
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
            var tables = await monitorService.ListTables(
                args.Subscription!,
                args.ResourceGroup!,
                args.Workspace!,
                args.TableType,
                args.Tenant,
                args.RetryPolicy);

            context.Response.Results = tables?.Count > 0 ?
                new { tables } :
                null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }

    protected ArgumentChain<TableListArguments> CreateTableTypeArgument()
    {
        var defaultValue = ArgumentDefinitions.Monitor.TableType.DefaultValue ?? "CustomLog";
        return ArgumentChain<TableListArguments>
            .Create(ArgumentDefinitions.Monitor.TableType.Name, ArgumentDefinitions.Monitor.TableType.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Monitor.TableType))
            .WithValueAccessor(args => args.TableType ?? defaultValue)
            .WithDefaultValue(defaultValue)
            .WithIsRequired(ArgumentDefinitions.Monitor.TableType.Required);
    }
}