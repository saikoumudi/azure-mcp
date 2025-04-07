using AzureMCP.Arguments.Extension;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Runtime.InteropServices;

namespace AzureMCP.Commands.Extension;

public class AzCommand : BaseCommand<AzArguments>
{
    private readonly Option<string> _commandOption = ArgumentDefinitions.Extension.Az.Command.ToOption();
    private readonly int _processTimeoutSeconds;
    private static string? _cachedAzPath;

    private static readonly string[] AzureCliPaths =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft SDKs", "Azure", "CLI2", "wbin"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft SDKs", "Azure", "CLI2", "wbin"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python39", "Scripts"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python310", "Scripts"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python311", "Scripts")
    ];

    public AzCommand(int processTimeoutSeconds = 300) : base()
    {
        _processTimeoutSeconds = processTimeoutSeconds;

        // Register the command argument in the chain
        AddArgumentToChain(CreateCommandArgument());
    }

    protected ArgumentChain<AzArguments> CreateCommandArgument() =>
        ArgumentChain<AzArguments>
            .Create(ArgumentDefinitions.Extension.Az.Command.Name, ArgumentDefinitions.Extension.Az.Command.Description)
            .WithCommandExample(ArgumentDefinitions.GetCommandExample(GetCommandPath(), ArgumentDefinitions.Extension.Az.Command))
            .WithValueAccessor(args => args.Command ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.Extension.Az.Command.Required);

    [McpServerTool(Destructive = true, ReadOnly = false)]
    public override Command GetCommand()
    {
        var command = new Command(
            "az",
            "Your job is to answer questions about an Azure environment by executing Azure CLI commands. You have the following rules:\n\n" +
            "- You should use the Azure CLI to manage Azure resources and services. Do not use any other tool.\n" +
            "- You should provide a valid Azure CLI command. For example: 'group list'.\n" +
            "- When deleting or modifying resources, ALWAYS request user confirmation.\n" +
            "- Whenever a command fails, retry it 3 times before giving up with an improved version of the code based on the returned feedback.\n" +
            "- When listing resources, ensure pagination is handled correctly so that all resources are returned.\n" +
            "- This tool can ONLY write code that interacts with Azure. It CANNOT generate charts, tables, graphs, etc.\n\n" +
            "- This tool can delete or modify resources in your Azure environment. Always be cautious and include appropriate warnings when providing commands to users.\n\n" +
            "Be concise, professional and to the point. Do not give generic advice, always reply with detailed & contextual data sourced from the current Azure environment.");

        command.AddOption(_commandOption);
        return command;
    }

    protected override AzArguments BindArguments(ParseResult parseResult)
    {
        var args = base.BindArguments(parseResult);
        args.Command = parseResult.GetValueForOption(_commandOption);
        return args;
    }

    private static string? FindAzCliPath()
    {
        // Return cached path if available and still exists
        if (!string.IsNullOrEmpty(_cachedAzPath) && File.Exists(_cachedAzPath))
        {
            return _cachedAzPath;
        }

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var searchPaths = new List<string>(AzureCliPaths);

        // Add PATH environment directories
        var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator);
        if (pathDirs != null)
        {
            searchPaths.AddRange(pathDirs);
        }

        foreach (var dir in searchPaths.Where(d => !string.IsNullOrEmpty(d)))
        {
            if (isWindows)
            {
                var cmdPath = Path.Combine(dir, "az.cmd");
                if (File.Exists(cmdPath))
                {
                    _cachedAzPath = cmdPath;
                    return cmdPath;
                }
            }
            else
            {
                var fullPath = Path.Combine(dir, "az");
                if (File.Exists(fullPath))
                {
                    _cachedAzPath = fullPath;
                    return fullPath;
                }
            }
        }

        return null;
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

            var command = options.Command ?? throw new ArgumentNullException(nameof(options.Command), "Command cannot be null");
            var processService = context.GetService<IExternalProcessService>();

            var azPath = FindAzCliPath() ?? throw new FileNotFoundException("Azure CLI executable not found in PATH or common installation locations. Please ensure Azure CLI is installed.");
            var result = await processService.ExecuteAsync(azPath, command, _processTimeoutSeconds);
            context.Response.Results = processService.ParseJsonOutput(result);
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}