using AzureMCP.Arguments.AppConfig.KeyValue;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.AppConfig.KeyValue;

public class KeyValueLockCommand : BaseAppConfigCommand<KeyValueLockArguments>
{
    private readonly Option<string> _keyOption;
    private readonly Option<string> _labelOption;

    public KeyValueLockCommand() : base()
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
            "lock",
            "Lock a key-value in an App Configuration store. This command sets a key-value to read-only mode, preventing any modifications to its value. You must specify an account name and key. Optionally, you can specify a label to lock a specific labeled version of the key-value.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_accountOption);
        command.AddOption(_keyOption);
        command.AddOption(_labelOption);

        return command;
    }

    protected override KeyValueLockArguments BindArguments(ParseResult parseResult)
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
            await appConfigService.LockKeyValue(
                options.Account!,
                options.Key!,
                options.Subscription!,
                options.TenantId,
                options.RetryPolicy,
                options.Label);

            context.Response.Results = new { key = options.Key, label = options.Label };
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}