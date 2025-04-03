using AzureMCP.Arguments.AppConfig.KeyValue;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.AppConfig.KeyValue;

public class KeyValueSetCommand : BaseKeyValueCommand<KeyValueSetArguments>
{
    private readonly Option<string> _keyOption;
    private readonly Option<string> _valueOption;
    private readonly Option<string> _labelOption;

    public KeyValueSetCommand() : base()
    {
        _keyOption = ArgumentDefinitions.AppConfig.Key.ToOption();
        _valueOption = ArgumentDefinitions.AppConfig.Value.ToOption();
        _labelOption = ArgumentDefinitions.AppConfig.Label.ToOption();

        RegisterArgumentChain(
            CreateAccountArgument(),
            CreateKeyArgument(),
            CreateValueArgument(),
            CreateLabelArgument()
        );
    }

    // Helper method for creating value arguments
    protected ArgumentChain<KeyValueSetArguments> CreateValueArgument()
    {
        return ArgumentChain<KeyValueSetArguments>
            .Create(ArgumentDefinitions.AppConfig.Value.Name, ArgumentDefinitions.AppConfig.Value.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.AppConfig.Value))
            .WithValueAccessor(args => args.Value ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.AppConfig.Value.Required);
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "set",
            "Set a key-value setting in an App Configuration store. This command creates or updates a key-value setting with the specified value. You must specify an account name, key, and value. Optionally, you can specify a label otherwise the default label will be used.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_accountOption);
        command.AddOption(_keyOption);
        command.AddOption(_valueOption);
        command.AddOption(_labelOption);

        return command;
    }

    protected override KeyValueSetArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Account = parseResult.GetValueForOption(_accountOption);
        args.Key = parseResult.GetValueForOption(_keyOption);
        args.Value = parseResult.GetValueForOption(_valueOption);
        args.Label = parseResult.GetValueForOption(_labelOption);
        return args;
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
            await appConfigService.SetKeyValue(
                options.Account!,
                options.Key!,
                options.Value!,
                options.Subscription!,
                options.TenantId,
                options.RetryPolicy,
                options.Label);

            context.Response.Results = new { key = options.Key, value = options.Value, label = options.Label };
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}