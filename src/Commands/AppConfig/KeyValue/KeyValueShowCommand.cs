using AzureMCP.Arguments.AppConfig.KeyValue;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.AppConfig.KeyValue;

public class KeyValueShowCommand : BaseKeyValueCommand<KeyValueShowArguments>
{
    private readonly Option<string> _keyOption;
    private readonly Option<string> _labelOption;

    public KeyValueShowCommand() : base()
    {
        _keyOption = ArgumentDefinitions.AppConfig.Key.ToOption();
        _labelOption = ArgumentDefinitions.AppConfig.Label.ToOption();

        RegisterArgumentChain(
            CreateAccountArgument(),
            CreateKeyArgument(),
            CreateLabelArgument()
        );
    }

    public override Command GetCommand()
    {
        var command = new Command(
            "show",
            "Show a specific key-value setting in an App Configuration store. This command retrieves and displays the value, label, content type, ETag, last modified time, and lock status for a specific setting. You must specify an account name and key. Optionally, you can specify a label otherwise the setting with default label will be retrieved.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_accountOption);
        command.AddOption(_keyOption);
        command.AddOption(_labelOption);

        return command;
    }

    protected override KeyValueShowArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Account = parseResult.GetValueForOption(_accountOption);
        args.Key = parseResult.GetValueForOption(_keyOption);
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
            var setting = await appConfigService.GetKeyValue(
                options.Account!,
                options.Key!,
                options.Subscription!,
                options.TenantId,
                options.RetryPolicy,
                options.Label);

            context.Response.Results = new { setting };
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}