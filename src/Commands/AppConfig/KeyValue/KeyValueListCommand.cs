using AzureMCP.Arguments.AppConfig.KeyValue;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.AppConfig.KeyValue;

public class KeyValueListCommand : BaseAppConfigCommand<KeyValueListArguments>
{
    private readonly Option<string> _keyOption;
    private readonly Option<string> _labelOption;

    public KeyValueListCommand() : base()
    {
        _keyOption = ArgumentDefinitions.AppConfig.KeyValueList.Key.ToOption();
        _labelOption = ArgumentDefinitions.AppConfig.KeyValueList.Label.ToOption();

        RegisterArgumentChain(
            CreateAccountArgument(),
            CreateListKeyArgument(),
            CreateListLabelArgument()
        );
    }

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override Command GetCommand()
    {
        var command = new Command(
            "list",
            "List all key-values in an App Configuration store. This command retrieves and displays all key-value pairs from the specified store. Each key-value includes its key, value, label, content type, ETag, last modified time, and lock status.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_accountOption);
        command.AddOption(_keyOption);
        command.AddOption(_labelOption);

        return command;
    }

    protected override KeyValueListArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Account = parseResult.GetValueForOption(_accountOption);
        args.Key = parseResult.GetValueForOption(_keyOption);
        args.Label = parseResult.GetValueForOption(_labelOption);
        return args;
    }

    private ArgumentChain<KeyValueListArguments> CreateListKeyArgument()
    {
        return ArgumentChain<KeyValueListArguments>
            .Create(ArgumentDefinitions.AppConfig.KeyValueList.Key.Name, ArgumentDefinitions.AppConfig.KeyValueList.Key.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.AppConfig.KeyValueList.Key))
            .WithValueAccessor(args => args.Key ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.AppConfig.KeyValueList.Key.Required);
    }

    private ArgumentChain<KeyValueListArguments> CreateListLabelArgument()
    {
        return ArgumentChain<KeyValueListArguments>
            .Create(ArgumentDefinitions.AppConfig.KeyValueList.Label.Name, ArgumentDefinitions.AppConfig.KeyValueList.Label.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.AppConfig.KeyValueList.Label))
            .WithValueAccessor(args => args.Label ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.AppConfig.KeyValueList.Label.Required);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindArguments(parseResult);

        try
        {
            if (!await ProcessArgumentChain(context, options))
            {
                return context.Response;
            }

            var appConfigService = context.GetService<IAppConfigService>();
            var settings = await appConfigService.ListKeyValues(
                options.Account!,
                options.Subscription!,
                options.Key,
                options.Label,
                options.TenantId,
                options.RetryPolicy);

            context.Response.Results = settings?.Count > 0 ?
                new { settings } :
                null;
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}