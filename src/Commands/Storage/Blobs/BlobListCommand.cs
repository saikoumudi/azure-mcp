using System.CommandLine;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using AzureMCP.Arguments.Storage.Blob;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Storage.Blob;

public class BlobListCommand : BaseStorageCommand<BlobListArguments>
{
    public BlobListCommand()
        : base()
    {
        RegisterArgumentChain(
            CreateAccountArgument(GetAccountOptions),
            CreateContainerArgument(GetContainerOptions)
        );
    }

    public override Command GetCommand()
    {
        var command = new Command("list", "List all blobs in a Storage container. This command retrieves and displays all blobs available in the specified container and Storage account. Results include blob names, sizes, and content types, returned as a JSON array. You must specify both an account name and a container name. Use this command to explore your container contents or to verify blob existence before performing operations on specific blobs.");
        AddBaseOptionsToCommand(command);
        command.AddOption(_accountOption);
        command.AddOption(_containerOption);
        return command;
    }

    protected override BlobListArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Account = parseResult.GetValueForOption(_accountOption);
        args.Container = parseResult.GetValueForOption(_containerOption);
        return args;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult commandOptions)
    {
        var options = BindArguments(commandOptions);

        try
        {
            if (!await ProcessArgumentChain(context, options))
            {
                return context.Response;
            }

            var storageService = context.GetService<IStorageService>();
            var blobs = await storageService.ListBlobs(
                options.Account!, 
                options.Container!, 
                options.SubscriptionId!, 
                options.TenantId,
                options.RetryPolicy);
                
            context.Response.Results = blobs?.Count > 0 ? new { blobs } : null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}
