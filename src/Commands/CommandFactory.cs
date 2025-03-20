using System.CommandLine;
using System.CommandLine.Invocation;
using AzureMCP.Commands.Cosmos;
using AzureMCP.Commands.Server;
using AzureMCP.Commands.Storage;
using AzureMCP.Commands.Subscriptions;

using AzureMCP.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Linq;
using AzureMCP.Commands.Storage.Blobs.Containers;
using AzureMCP.Commands.Capabilities;
using AzureMCP.Commands.Storage.Blobs;
using System.Text;

namespace AzureMCP.Commands;

public class CommandFactory
{
    
    private readonly IServiceProvider _serviceProvider;
    private readonly RootCommand _rootCommand;
    private readonly CommandGroup _rootGroup;
    
    internal static readonly char Separator = '-';

    /// <summary>
    /// Mapping of hyphenated command names to their <see cref="ICommand" />
    /// </summary>
    private readonly Dictionary<string, ICommand> _commandMap;

    public CommandFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _rootGroup = new CommandGroup("azmcp", "Azure MCP Server");
        _rootCommand = CreateRootCommand();
        _commandMap = CreateCommmandDictionary(_rootGroup, string.Empty);
    }

    public RootCommand RootCommand => _rootCommand;

    public CommandGroup RootGroup => _rootGroup;

    public IReadOnlyDictionary<string, ICommand> AllCommands => _commandMap;

    private void RegisterCommandGroups()
    {
        // Register top-level command groups
        RegisterCosmosCommands();
        RegisterStorageCommands();
        RegisterMonitorCommands();
        RegisterCapabilitiesCommands();
        RegisterSubscriptionsCommands();
        RegisterMcpServerCommands();
    }

    private void RegisterCosmosCommands()
    {
        // Create Cosmos command group
        var cosmos = new CommandGroup("cosmos", "Cosmos DB operations - Commands for managing and querying Azure Cosmos DB resources. Includes operations for databases, containers, and document queries.");
        _rootGroup.AddSubGroup(cosmos);

        // Create Cosmos subgroups
        var databases = new CommandGroup("databases", "Cosmos DB databases operations - Commands for listing, creating, and managing databases within your Cosmos DB accounts.");
        cosmos.AddSubGroup(databases);

        var cosmosContainers = new CommandGroup("containers", "Cosmos DB containers operations - Commands for listing, creating, and managing containers (collections) within your Cosmos DB databases.");
        databases.AddSubGroup(cosmosContainers);

        var cosmosAccounts = new CommandGroup("accounts", "Cosmos DB accounts operations - Commands for listing and managing Cosmos DB accounts in your Azure subscription.");
        cosmos.AddSubGroup(cosmosAccounts);

        // Create items subgroup for Cosmos
        var cosmosItems = new CommandGroup("items", "Cosmos DB items operations - Commands for querying, creating, updating, and deleting documents within your Cosmos DB containers.");
        cosmosContainers.AddSubGroup(cosmosItems);

        // Register Cosmos commands
        databases.AddCommand("list", new DatabasesListCommand());
        cosmosContainers.AddCommand("list", new Cosmos.ContainersListCommand());
        cosmosAccounts.AddCommand("list", new Cosmos.AccountsListCommand());
        cosmosItems.AddCommand("query", new ItemsQueryCommand());


    }

    private void RegisterStorageCommands()
    {
        // Create Storage command group
        var storage = new CommandGroup("storage", "Storage operations - Commands for managing and accessing Azure Storage resources. Includes operations for containers, blobs, and tables.");
        _rootGroup.AddSubGroup(storage);

        // Create Storage subgroups
        var storageAccounts = new CommandGroup("accounts", "Storage accounts operations - Commands for listing and managing Storage accounts in your Azure subscription.");
        storage.AddSubGroup(storageAccounts);

        var tables = new CommandGroup("tables", "Storage tables operations - Commands for working with Azure Table Storage, including listing and querying tables.");
        storage.AddSubGroup(tables);

        var blobs = new CommandGroup("blobs", "Storage blobs operations - Commands for uploading, downloading, and managing blobs in your Azure Storage accounts.");
        storage.AddSubGroup(blobs);

        // Create a containers subgroup under blobs
        var blobContainers = new CommandGroup("containers", "Storage blob containers operations - Commands for managing blob containers in your Azure Storage accounts.");
        blobs.AddSubGroup(blobContainers);

        // Register Storage commands
        storageAccounts.AddCommand("list", new Storage.Accounts.AccountsListCommand());
        tables.AddCommand("list", new Storage.Tables.TablesListCommand());
        blobs.AddCommand("list", new BlobsListCommand());
        blobContainers.AddCommand("list", new Storage.Blobs.Containers.ContainersListCommand());
        blobContainers.AddCommand("details", new Storage.Blobs.Containers.ContainersDetailsCommand());
    }

    private void RegisterMonitorCommands()
    {
        // Create Monitor command group
        var monitor = new CommandGroup("monitor", "Azure Monitor operations - Commands for querying and analyzing Azure Monitor logs and metrics.");
        _rootGroup.AddSubGroup(monitor);

        // Create Monitor subgroups
        var logs = new CommandGroup("logs", "Azure Monitor logs operations - Commands for querying Log Analytics workspaces using KQL.");
        monitor.AddSubGroup(logs);

        var workspaces = new CommandGroup("workspaces", "Log Analytics workspace operations - Commands for managing Log Analytics workspaces.");
        monitor.AddSubGroup(workspaces);

        var monitorTables = new CommandGroup("tables", "Log Analytics workspace table operations - Commands for listing tables in Log Analytics workspaces.");
        monitor.AddSubGroup(monitorTables);

        // Register Monitor commands
        logs.AddCommand("query", new Monitor.Logs.LogsQueryCommand());
        workspaces.AddCommand("list", new Monitor.Workspaces.WorkspacesListCommand());
        monitorTables.AddCommand("list", new Monitor.Tables.TablesListCommand());


    }

    private void RegisterCapabilitiesCommands()
    {
        // Create Capabilities command group
        var capabilities = new CommandGroup("capabilities", "CLI capabilities operations - Commands for discovering and exploring the functionality available in this CLI tool.");
        _rootGroup.AddSubGroup(capabilities);

        // Register Capabilities commands
        capabilities.AddCommand("list", new CapabilitiesListCommand());

    }

    private void RegisterSubscriptionsCommands()
    {
        // Create Subscriptions command group
        var subscriptions = new CommandGroup("subscriptions", "Azure subscription operations - Commands for listing and managing Azure subscriptions accessible to your account.");
        _rootGroup.AddSubGroup(subscriptions);

        // Register Subscriptions commands
        subscriptions.AddCommand("list", new SubscriptionsListCommand());

    }

    private void RegisterMcpServerCommands()
    {
        // Create MCP Server command group
        var mcpServer = new CommandGroup("server", "MCP server operations - Commands for managing and interacting with the MCP server.");
        _rootGroup.AddSubGroup(mcpServer);

        // Register MCP Server commands
        var startServer = new McpStartServerCommand(_serviceProvider);
        mcpServer.AddCommand("start", startServer);

    }

    private void ConfigureCommands(CommandGroup group)
    {
        // Configure direct commands in this group
        foreach (var command in group.Commands.Values)
        {
            var cmd = command.GetCommand();

            if (cmd.Handler == null)
            {
                ConfigureCommandHandler(cmd, command);
            }

            group.Command.Add(cmd);
        }

        // Recursively configure subgroup commands
        foreach (var subGroup in group.SubGroups)
        {
            ConfigureCommands(subGroup);
        }
    }

    private RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand("Azure AI Data Plane CLI - A comprehensive command-line interface for interacting with Azure data services. This CLI provides direct access to Azure data plane operations, allowing you to manage and query your Azure resources efficiently without switching between multiple tools.");

        RegisterCommandGroups();

        // Copy the root group's subcommands to the RootCommand
        foreach (var subGroup in _rootGroup.SubGroups)
        {
            rootCommand.Add(subGroup.Command);
        }

        // Configure all commands in the hierarchy
        ConfigureCommands(_rootGroup);

        return rootCommand;
    }

    private void ConfigureCommandHandler(Command command, ICommand implementation)
    {
        command.SetHandler(async (InvocationContext context) =>
        {
            var startTime = DateTime.UtcNow;
            var cmdContext = new CommandContext(_serviceProvider);
            var response = await implementation.ExecuteAsync(cmdContext, context.ParseResult);

            // Calculate execution time
            var endTime = DateTime.UtcNow;
            response.Duration = (long)(endTime - startTime).TotalMilliseconds;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            Console.WriteLine(JsonSerializer.Serialize(response, options));
        });
    }

    private ICommand? FindCommandInGroup(CommandGroup group, Queue<string> nameParts)
    {
        // If we've processed all parts and this group has a matching command, return it
        if (nameParts.Count == 1)
        {
            var commandName = nameParts.Dequeue();
            return group.Commands.GetValueOrDefault(commandName);
        }

        // Find the next subgroup
        var groupName = nameParts.Dequeue();
        var nextGroup = group.SubGroups.FirstOrDefault(g => g.Name == groupName);

        return nextGroup != null ? FindCommandInGroup(nextGroup, nameParts) : null;
    }

    public ICommand? FindCommandByName(string hyphenatedName)
    {
        return _commandMap.GetValueOrDefault(hyphenatedName);
    }

    private Dictionary<string, ICommand> CreateCommmandDictionary(CommandGroup node, string prefix)
    {
        var aggregated = new Dictionary<string, ICommand>();
        var updatedPrefix = GetPrefix(prefix, node.Name);

        if (node.Commands != null)
        {
            foreach (var kvp in node.Commands)
            {
                var key = GetPrefix(updatedPrefix, kvp.Key);

                aggregated.Add(key, kvp.Value);
            }
        }

        if (node.SubGroups == null)
        {
            return aggregated;
        }
        
        foreach (var command in node.SubGroups)
        {
            var childPrefix = GetPrefix(updatedPrefix, command.Name);
            var subcommandsDictionary = CreateCommmandDictionary(command, updatedPrefix);

            foreach (var item in subcommandsDictionary)
            {
                aggregated.Add(item.Key, item.Value);
            }
        }

        return aggregated;
    }

    private static string GetPrefix(string currentPrefix, string additional) => string.IsNullOrEmpty(currentPrefix)
        ? additional
        : currentPrefix + Separator + additional;
}
