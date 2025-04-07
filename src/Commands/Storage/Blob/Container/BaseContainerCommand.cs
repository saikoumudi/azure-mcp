using AzureMCP.Commands.Storage;
using AzureMCP.Models.Argument;

public abstract class BaseContainerCommand<TArgs> : BaseStorageCommand<TArgs> where TArgs : BaseContainerArguments, new()
{
    protected BaseContainerCommand()
    {
    }

    protected ArgumentChain<TArgs> CreateContainerArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Storage.Container.Name, ArgumentDefinitions.Storage.Container.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Storage.Container))
            .WithValueAccessor(args => args.Container ?? string.Empty)
            .WithValueLoader(GetContainerOptions)
            .WithIsRequired(ArgumentDefinitions.Storage.Container.Required);
    }
}