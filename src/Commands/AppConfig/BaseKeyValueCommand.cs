using AzureMCP.Arguments.AppConfig.KeyValue;
using AzureMCP.Models;

namespace AzureMCP.Commands.AppConfig;

public abstract class BaseKeyValueCommand<TArgs> : BaseAppConfigCommand<TArgs> where TArgs : BaseKeyValueArguments, new()
{

    // Helper method for creating key arguments
    protected ArgumentChain<TArgs> CreateKeyArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.AppConfig.Key.Name, ArgumentDefinitions.AppConfig.Key.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.AppConfig.Key))
            .WithValueAccessor(args => args.Key ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.AppConfig.Key.Required);
    }

    // Helper method for creating label arguments
    protected ArgumentChain<TArgs> CreateLabelArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.AppConfig.Label.Name, ArgumentDefinitions.AppConfig.Label.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.AppConfig.Label))
            .WithValueAccessor(args => args.Label ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.AppConfig.Label.Required);
    }
}