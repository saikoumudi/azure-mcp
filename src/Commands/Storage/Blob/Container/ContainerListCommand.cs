using AzureMCP.Arguments.Storage.Blob.Container;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Storage.Blob.Container;

public class ContainerListCommand : BaseStorageCommand<ContainerListArguments>
{
    public ContainerListCommand() : base()
    {
        RegisterArgumentChain(
            CreateAccountArgument()
        );
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override Command GetCommand()
    {
        var command = new Command("list", "List all containers in the specified storage account.");

        // Add base options which includes subscription and auth options
        AddBaseOptionsToCommand(command);
        command.AddOption(_accountOption);
        return command;
    }

    protected override ContainerListArguments BindArguments(ParseResult parseResult)
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
            var containers = await storageService.ListContainers(args.Account!, args.Subscription!, args.Tenant,
                args.RetryPolicy);

            context.Response.Results = containers?.Count > 0 ? new { containers } : null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}