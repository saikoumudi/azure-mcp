using AzureMCP.Arguments.Subscription;

namespace AzureMCP.Commands.Subscription;

public abstract class BaseSubscriptionCommand<TArgs> : BaseCommand<TArgs>
    where TArgs : BaseSubscriptionArguments, new()
{
    protected BaseSubscriptionCommand()
    {
        // No additional options needed for base subscriptions command
    }
}