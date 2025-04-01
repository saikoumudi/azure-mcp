using AzureMCP.Arguments.Storage.Blob.Container;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Storage.Blob.Container;

public class ContainerDetailsCommand : BaseStorageCommand<ContainerDetailsArguments>
{
    public ContainerDetailsCommand() : base()
    {
        // Create the argument chain with account and container arguments
        RegisterArgumentChain(
            CreateAccountArgument(),
            CreateContainerArgument()
        );
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "details",
            "Get detailed properties of a storage container including metadata, lease status, and access level. " +
            $"Requires {ArgumentDefinitions.Storage.AccountName} and {ArgumentDefinitions.Storage.ContainerName}.");

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
                args.Subscription!,
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
