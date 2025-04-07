using AzureMCP.Arguments;
using AzureMCP.Models;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureMCP.Commands.Tools;

[HiddenCommand]
public class ToolsListCommand : BaseCommandWithoutArgs
{
    [McpServerTool(Destructive = false, ReadOnly = true)]
    public override Command GetCommand()
    {
        return new Command("list", "List all available commands and their tools in a hierarchical structure. This command returns detailed information about each command, including its name, description, full command path, available subcommands, and all supported arguments. Use this to explore the CLI's functionality or to build interactive command interfaces.");
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var factory = context.GetService<CommandFactory>();
            var tools = factory.AllCommands
                .Where(kvp =>
                {
                    var parts = kvp.Key.Split(CommandFactory.Separator, StringSplitOptions.RemoveEmptyEntries);
                    return !parts.Any(x => x == "tools" || x == "server");
                })
                .Select(kvp => CreateCommand(kvp.Key, kvp.Value))
                .ToList();

            context.Response.Results = tools;
            return Task.FromResult(context.Response);
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
            return Task.FromResult(context.Response);
        }
    }

    private static CommandInfo CreateCommand(string hyphenatedName, ICommand command)
    {
        var fullPath = hyphenatedName.Replace(CommandFactory.Separator, ' ');
        var argumentInfos = command.GetArgumentChain()
            ?.Select(arg =>
            {
                return new ArgumentInfo(
                    name: arg.Name,
                    description: arg.Description,
                    command: fullPath,
                    value: string.Empty,
                    defaultValue: arg is ArgumentChain<BaseArguments> chain ? chain.DefaultValue : null,
                    values: arg is ArgumentChain<BaseArguments> chainWithValues ? chainWithValues.Values : null,
                    required: arg.Required
                    );
            })
            .ToList();

        return new CommandInfo
        {
            Name = command.GetCommand().Name,
            Description = command.GetCommand().Description ?? string.Empty,
            FullPath = hyphenatedName.Replace('-', ' '),
            Arguments = argumentInfos,
        };
    }
}