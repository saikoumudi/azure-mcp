using AzureMCP.Arguments.AppConfig.KeyValue;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.AppConfig.KeyValue;

public sealed class KeyValueShowCommand : BaseKeyValueCommand<KeyValueShowArguments>
{
    protected override string GetCommandName() => "show";

    protected override string GetCommandDescription() =>
        """
        Show a specific key-value setting in an App Configuration store. This command retrieves and displays the value, 
        label, content type, ETag, last modified time, and lock status for a specific setting. You must specify an 
        account name and key. Optionally, you can specify a label otherwise the setting with default label will be retrieved.
        """;

    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            if (!await ProcessArguments(context, args))
            {
                return context.Response;
            }

            var appConfigService = context.GetService<IAppConfigService>();
            var setting = await appConfigService.GetKeyValue(
                args.Account!,
                args.Key!,
                args.Subscription!,
                args.Tenant,
                args.RetryPolicy,
                args.Label);

            context.Response.Results = new { setting };
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}