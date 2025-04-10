using AzureMCP.Arguments;
using AzureMCP.Models.Argument;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands;

public abstract class SubscriptionCommand<TArgs> : GlobalCommand<TArgs>
    where TArgs : SubscriptionArguments, new()
{
    protected readonly Option<string> _subscriptionOption = ArgumentDefinitions.Common.Subscription.ToOption();

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_subscriptionOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateSubscriptionArgument());
    }

    protected ArgumentBuilder<TArgs> CreateSubscriptionArgument()
    {
        return ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Common.Subscription.Name, ArgumentDefinitions.Common.Subscription.Description)
            .WithValueAccessor(args => args.Subscription ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.Common.Subscription.Required);
    }

    protected override TArgs BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Subscription = parseResult.GetValueForOption(_subscriptionOption);
        return args;
    }

    protected ArgumentBuilder<TArgs> CreateResourceGroupArgument()
    {

        return ArgumentBuilder<TArgs>
            .Create(ArgumentDefinitions.Common.ResourceGroup.Name, ArgumentDefinitions.Common.ResourceGroup.Description)
            .WithValueAccessor(args => (args as SubscriptionArguments)?.ResourceGroup ?? string.Empty)
            .WithSuggestedValuesLoader(async (context, args) =>
            {
                var subArgs = args as SubscriptionArguments;
                if (string.IsNullOrEmpty(subArgs?.Subscription))
                {
                    return [];
                }
                return await GetResourceGroupOptions(context, subArgs.Subscription);
            })
            .WithIsRequired(true);
    }
}