using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMCP.Arguments.Storage.Blob.Container;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;

namespace AzureMCP.Commands.Storage.Blob.Container;

public class ContainerDetailsCommand : BaseStorageCommand<ContainerDetailsArguments>
{
    public ContainerDetailsCommand() : base()
    {
        // Create the argument chain with account and container arguments
        RegisterArgumentChain(
            CreateAccountArgument(GetAccountOptions),
            CreateContainerArgument(GetContainerOptions)
        );
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "details",
            "Get detailed properties of a storage container including metadata, lease status, and access level. " + 
            "Requires storage account name and container name.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_accountOption);
        command.AddOption(_containerOption);
        return command;
    }

    protected override ContainerDetailsArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        
        // Explicitly bind the options we care about
        args.Account = parseResult.GetValueForOption(_accountOption);
        args.Container = parseResult.GetValueForOption(_containerOption);
        
        return args;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var args = BindArguments(parseResult);
            
            if (!await ProcessArgumentChain(context, args))
            {
                return context.Response;
            }

            var storageService = context.GetService<IStorageService>();
            var details = await storageService.GetContainerDetails(
                args.Account!,
                args.Container!,
                args.SubscriptionId!,
                args.TenantId,
                args.RetryPolicy
            );

            context.Response.Results = new { details };
            return context.Response;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
            return context.Response;
        }
    }
}
