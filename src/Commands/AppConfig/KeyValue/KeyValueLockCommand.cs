using AzureMCP.Arguments.AppConfig.KeyValue;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.AppConfig.KeyValue;

public class KeyValueLockCommand : BaseKeyValueCommand<KeyValueLockArguments>
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

    [McpServerTool(Destructive = false, ReadOnly = false)]
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
        var args = BindArguments(parseResult);

        try
        {
            if (!await ProcessArgumentChain(context, args))
            {
                return context.Response;
            }

            var appConfigService = context.GetService<IAppConfigService>();
            await appConfigService.LockKeyValue(
                args.Account!,
                args.Key!,
                args.Subscription!,
                args.Tenant,
                args.RetryPolicy,
                args.Label);

            context.Response.Results = new { key = args.Key, label = args.Label };
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}