using System.CommandLine;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using AzureMCP.Arguments.Subscription;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.CommandLine.Parsing;
using AzureMCP.Extensions;
using AzureMCP.Arguments;

namespace AzureMCP.Commands.Subscriptions;

public class SubscriptionsListCommand : BaseSubscriptionsCommand<SubscriptionListArguments>
{
    public SubscriptionsListCommand() : base()
    {
        // Define the argument chain with required and optional arguments
        // Note: We're not calling RegisterArgumentChain here because we don't want to include
        // the subscription-id parameter that's automatically added by BaseCommand
        
        // Instead, we'll manually add only the tenant ID argument
        _argumentChain.Clear(); // Clear any existing arguments
        _argumentChain.Add(CreateTenantIdArgument());
        // Deliberately NOT adding subscription ID or auth method arguments
    }

    public override Command GetCommand()
    {
        var command = new Command("list", "List all Azure subscriptions accessible to your account. This command retrieves and displays all subscriptions that your authenticated identity has access to. Results include subscription names and IDs, returned as a JSON array. This command is useful for identifying available subscriptions before performing operations that require a subscription ID.");
        
        // Add both tenant and auth method options
        command.AddOption(_tenantOption);
        command.AddOption(_authMethodOption);
        
        // Add retry options
        AddRetryOptionsToCommand(command);
        
        return command;
    }

    protected override SubscriptionListArguments BindArguments(ParseResult parseResult)
    {
        var args = new SubscriptionListArguments();

        if (parseResult != null)
        {
            args.TenantId = parseResult.GetValueForOption(_tenantOption);
            args.AuthMethod = parseResult.GetValueForOption(_authMethodOption);

            if (parseResult.HasAnyRetryOptions())
            {
                args.RetryPolicy = GetRetryPolicyArguments(parseResult);
            }
        }

        return args;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult commandOptions)
    {
        var options = BindArguments(commandOptions);

        try
        {
            // Process the argument chain and return early if any required arguments are missing
            if (!await ProcessArgumentChain(context, options))
            {
                return context.Response;
            }

            // All required arguments are provided, execute the command
            var subscriptionService = context.GetService<ISubscriptionService>();
            var subscriptions = await subscriptionService.GetSubscriptions(options.TenantId,
                options.RetryPolicy);
            
            context.Response.Results = subscriptions?.Count > 0 ? new { subscriptions } : null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}