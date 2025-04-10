using AzureMCP.Arguments.Extension;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Runtime.InteropServices;

namespace AzureMCP.Commands.Extension;

public sealed class AzCommand(int processTimeoutSeconds = 300) : GlobalCommand<AzArguments>
{
    private readonly Option<string> _commandOption = ArgumentDefinitions.Extension.Az.Command.ToOption();
    private static string? _cachedAzPath;

    private static readonly string[] AzureCliPaths =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft SDKs", "Azure", "CLI2", "wbin"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft SDKs", "Azure", "CLI2", "wbin"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python39", "Scripts"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python310", "Scripts"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python311", "Scripts")
    ];

    protected override string GetCommandName() => "az";

    protected override string GetCommandDescription() =>
        """
        Your job is to answer questions about an Azure environment by executing Azure CLI commands. You have the following rules:

        - You should use the Azure CLI to manage Azure resources and services. Do not use any other tool.
        - You should provide a valid Azure CLI command. For example: 'group list'.
        - When deleting or modifying resources, ALWAYS request user confirmation.
        - Whenever a command fails, retry it 3 times before giving up with an improved version of the code based on the returned feedback.
        - When listing resources, ensure pagination is handled correctly so that all resources are returned.
        - This tool can ONLY write code that interacts with Azure. It CANNOT generate charts, tables, graphs, etc.
        - This tool can delete or modify resources in your Azure environment. Always be cautious and include appropriate warnings when providing commands to users.

        Be concise, professional and to the point. Do not give generic advice, always reply with detailed & contextual data sourced from the current Azure environment.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_commandOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(CreateCommandArgument());
    }

    private static ArgumentBuilder<AzArguments> CreateCommandArgument() =>
        ArgumentBuilder<AzArguments>
            .Create(ArgumentDefinitions.Extension.Az.Command.Name, ArgumentDefinitions.Extension.Az.Command.Description)
            .WithValueAccessor(args => args.Command ?? string.Empty)
            .WithIsRequired(ArgumentDefinitions.Extension.Az.Command.Required);

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

    [McpServerTool(Destructive = true, ReadOnly = false)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var args = BindArguments(parseResult);

        try
        {
            if (!await ProcessArguments(context, args))
            {
                return context.Response;
            }

            var command = args.Command ?? throw new ArgumentNullException(nameof(args.Command), "Command cannot be null");
            var processService = context.GetService<IExternalProcessService>();

            var azPath = FindAzCliPath() ?? throw new FileNotFoundException("Azure CLI executable not found in PATH or common installation locations. Please ensure Azure CLI is installed.");
            var result = await processService.ExecuteAsync(azPath, command, processTimeoutSeconds);
            context.Response.Results = processService.ParseJsonOutput(result);
        }
        catch (Exception ex)
        {
            HandleException(context.Response, ex);
        }

        return context.Response;
    }
}