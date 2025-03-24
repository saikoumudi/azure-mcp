using AzureMCP.Arguments.Monitor;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
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
           CreateWorkspaceNameArgument(GetWorkspaceOptions),
           CreateResourceGroupArgument()!,
           CreateTableTypeArgument()
       );
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "list",
            "List tables in a Log Analytics workspace. This command retrieves all available tables from the specified workspace, which can be used for constructing KQL queries. Table contain different types of log data depending on the solutions and data sources configured for your workspace.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_workspaceNameOption);
        command.AddOption(_resourceGroupOption);
        command.AddOption(_tableTypeOption);
        return command;
    }

    protected override TableListArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.WorkspaceName = parseResult.GetValueForOption(_workspaceNameOption);
        args.ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption);
        args.TableType = parseResult.GetValueForOption(_tableTypeOption);
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
            var tables = await monitorService.ListTables(
                options.SubscriptionId!,
                options.ResourceGroup!,
                options.WorkspaceName!,
                options.TableType,
                options.TenantId,
                options.RetryPolicy);

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
        return ArgumentChain<TableListArguments>
            .Create(ArgumentDefinitions.Monitor.TableTypeName, ArgumentDefinitions.Monitor.TableType.Description)
            .WithCommandExample($"{GetCommandPath()} --table-type <table-type>")
            .WithValueAccessor(args => args.TableType ?? string.Empty)
            .WithIsRequired(true);
    }
}
