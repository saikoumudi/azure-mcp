using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMCP.Arguments.Storage;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;

namespace AzureMCP.Commands.Storage.Accounts;

public class AccountsListCommand : BaseCommand<AccountsListArguments>
{
    public AccountsListCommand() : base()
    {
        RegisterArgumentChain();
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "list", 
            "List all Storage accounts in a subscription. This command retrieves and displays all Storage accounts available in the specified subscription. Results include account names and are returned as a JSON array. You must specify a subscription ID.");
            
        // We only need auth/subscription options for list command
        AddCommonOptionsToCommand(command);
        return command;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var args = BindArguments(parseResult);
            
            // Process argument chain and return early if required arguments are missing
            if (!await ProcessArgumentChain(context, args))
            {
                return context.Response;
            }

            var storageService = context.GetService<IStorageService>();
            var accounts = await storageService.GetStorageAccounts(
                args.SubscriptionId!,
                args.TenantId,
                args.RetryPolicy);
            
            context.Response.Results = accounts?.Count > 0 ? 
                new { accounts } : 
                null;
                
            return context.Response;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
            return context.Response;
        }
    }
}