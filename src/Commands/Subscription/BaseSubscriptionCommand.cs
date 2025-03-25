using AzureMCP.Arguments.Subscription;

namespace AzureMCP.Commands.Subscription;

public abstract class BaseSubscriptionsCommand<TArgs> : BaseCommand<TArgs>
    where TArgs : BaseSubscriptionArguments, new()
{
    protected BaseSubscriptionsCommand()
    {
        // No additional options needed for base subscriptions command
    }
}
