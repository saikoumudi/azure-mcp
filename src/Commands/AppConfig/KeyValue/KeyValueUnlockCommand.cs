using AzureMCP.Arguments.AppConfig.KeyValue;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.AppConfig.KeyValue;

public class KeyValueUnlockCommand : BaseKeyValueCommand<KeyValueUnlockArguments>
{
    private readonly Option<string> _keyOption;
    private readonly Option<string> _labelOption;

    public KeyValueUnlockCommand() : base()
    {
        _keyOption = ArgumentDefinitions.AppConfig.Key.ToOption();
        _labelOption = ArgumentDefinitions.AppConfig.Label.ToOption();

        RegisterArgumentChain(
            CreateAccountArgument(),
            CreateKeyArgument(),
            CreateLabelArgument()
        );
    }

    [McpServerTool(Destructive = false, ReadOnly = false)]
    public override Command GetCommand()
    {
        var command = new Command(
            "unlock",
            "Unlock a key-value setting in an App Configuration store. This command removes the read-only mode from a key-value setting, allowing modifications to its value. You must specify an account name and key. Optionally, you can specify a label to unlock a specific labeled version of the setting, otherwise the setting with the default label will be unlocked.");

        AddBaseOptionsToCommand(command);
        command.AddOption(_accountOption);
        command.AddOption(_keyOption);
        command.AddOption(_labelOption);

        return command;
    }

    protected override KeyValueUnlockArguments BindArguments(ParseResult parseResult)
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
            await appConfigService.UnlockKeyValue(
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