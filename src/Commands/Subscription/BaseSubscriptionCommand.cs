using AzureMCP.Arguments.Subscription;

namespace AzureMCP.Commands.Subscription;

public abstract class BaseSubscriptionsCommand<TArgs> : BaseCommand<TArgs>
    where TArgs : BaseSubscriptionArguments, new()
{
    protected BaseSubscriptionsCommand()
    {
        // No additional options needed for base subscriptions command
    }

    // Override to provide the correct command path for examples
    protected override string GetCommandPath()
    {
        // Extract the command name from the class name (e.g., SubscriptionsListCommand -> subscriptions list)
        string commandName = GetType().Name.Replace("Command", "");

        // Insert spaces before capital letters and convert to lowercase
        string formattedName = string.Concat(commandName.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).Trim();

        // Convert to lowercase
        string lowerName = formattedName.ToLowerInvariant();

        // Return the full command path
        return lowerName;
    }
}
