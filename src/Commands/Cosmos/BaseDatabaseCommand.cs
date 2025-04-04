
using AzureMCP.Arguments.Cosmos;
using AzureMCP.Models;
using System.CommandLine;

namespace AzureMCP.Commands.Cosmos;

public abstract class BaseDatabaseCommand<TArgs> : BaseCosmosCommand<TArgs> where TArgs : BaseDatabaseArguments, new()
{
    protected readonly Option<string> _databaseOption;

    protected BaseDatabaseCommand() : base()
    {
        _databaseOption = ArgumentDefinitions.Cosmos.Database.ToOption();
    }

    protected ArgumentChain<TArgs> CreateDatabaseArgument()
    {
        return ArgumentChain<TArgs>
            .Create(ArgumentDefinitions.Cosmos.Database.Name, ArgumentDefinitions.Cosmos.Database.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Cosmos.Database))
            .WithValueAccessor(args => args.Database ?? string.Empty)
            .WithValueLoader(async (context, args) =>
                await GetDatabaseOptions(context, args.Account ?? string.Empty, args.Subscription ?? string.Empty))
            .WithIsRequired(ArgumentDefinitions.Cosmos.Database.Required);
    }
}