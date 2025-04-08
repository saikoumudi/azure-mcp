using AzureMCP.Arguments.Storage.Table;
using AzureMCP.Models;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Storage.Table;

public class TableListCommand : BaseStorageCommand<TableListArguments>
{
    public TableListCommand() : base()
    {
        RegisterArgumentChain(
            CreateAccountArgument()
        );
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override Command GetCommand()
    {
        var command = new Command("list", "List all tables in a Storage account. This command retrieves and displays all tables available in the specified Storage account. Results include table names and are returned as a JSON array. You must specify an account name and subscription ID. Use this command to explore your Storage resources or to verify table existence before performing operations on specific tables.");
        AddBaseOptionsToCommand(command);
        command.AddOption(_accountOption);
        return command;
    }

    protected override TableListArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Account = parseResult.GetValueForOption(_accountOption);
        return args;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult commandOptions)
    {
        var args = BindArguments(commandOptions);

        try
        {
            if (!await ProcessArgumentChain(context, args))
            {
                return context.Response;
            }

            var storageService = context.GetService<IStorageService>();
            var tables = await storageService.ListTables(
                args.Account!,
                args.Subscription!,
                args.AuthMethod ?? AuthMethod.Credential,
                null,
                args.Tenant,
                args.RetryPolicy);

            context.Response.Results = tables?.Count > 0 ? new { tables } : null;

            // Only show warning if we actually had to fall back to a different auth method
            if (context.Response.Results != null && !string.IsNullOrEmpty(context.Response.Message))
            {
                var authMethod = args.AuthMethod ?? AuthMethod.Credential;

                if (authMethod == AuthMethod.Credential)
                {
                    if (context.Response.Message.Contains("connection string"))
                    {
                        context.Response.Message =
                            "Warning: Credential and key auth failed, succeeded using connection string. " +
                            "Consider using --auth-method connectionString for future calls.";
                    }
                }
                else if (authMethod == AuthMethod.Key && context.Response.Message.Contains("connection string"))
                {
                    context.Response.Message =
                        "Warning: Key auth failed, succeeded using connection string. " +
                        "Consider using --auth-method connectionString for future calls.";
                }
            }
            else
            {
                // Clear any warning message if auth succeeded directly
                context.Response.Message = string.Empty;
            }
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}