using AzureMCP.Arguments.Server;
using AzureMCP.Models;
using AzureMCP.Models.Argument;
using AzureMCP.Models.Command;
using AzureMCP.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;

namespace AzureMCP.Commands.Server;

[HiddenCommand]
public class ServiceStartCommand : ICommand
{
    private readonly Command _command;
    private readonly IServiceProvider _rootServiceProvider;
    private readonly List<ArgumentDefinition<string>> _arguments = [];
    private readonly Option<string> _transportOption;
    private readonly Option<int> _portOption;

    public ServiceStartCommand(IServiceProvider serviceProvider)
    {
        _arguments.Add(ArgumentDefinitions.Service.Transport);
        _arguments.Add(GetPortArgument());

        _transportOption = ArgumentDefinitions.Service.Transport.ToOption();
        _portOption = ArgumentDefinitions.Service.Port.ToOption();

        _rootServiceProvider = serviceProvider;

        _command = new Command("start", "Starts Azure MCP Server.");
        _command.AddOption(_transportOption);
        _command.AddOption(_portOption);
        _command.SetHandler(parsingContext => ExecuteAsync(new CommandContext(_rootServiceProvider), parsingContext.ParseResult));
    }

    public async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult commandOptions)
    {
        var port = commandOptions.GetValueForOption(_portOption) == default
            ? ArgumentDefinitions.Service.Port.DefaultValue
            : commandOptions.GetValueForOption(_portOption);
        var serverOptions = new ServiceStartArguments
        {
            Transport = commandOptions.GetValueForOption(_transportOption) ?? TransportTypes.StdIo,
            Port = port
        };

        using var host = CreateHost(serverOptions);

        await host.StartAsync(CancellationToken.None);

        // Wait for the host to be terminated
        await host.WaitForShutdownAsync(CancellationToken.None);

        return context.Response;
    }

    public Command GetCommand() => _command;

    private IHost CreateHost(ServiceStartArguments serverArguments)
    {
        if (serverArguments.Transport == TransportTypes.Sse)
        {
            var builder = WebApplication.CreateBuilder([]);
            ConfigureServices(builder.Services, _rootServiceProvider);
            ConfigureMcpServer(builder.Services, serverArguments.Transport);

            builder.WebHost.ConfigureKestrel(server =>
            {
                server.ListenLocalhost(serverArguments.Port);
            });

            var application = builder.Build();

            application.MapMcpSse();

            return application;
        }
        else
        {
            return Host.CreateDefaultBuilder()
                .ConfigureLogging(logging => logging.ClearProviders())
                .ConfigureServices(services =>
                {
                    ConfigureServices(services, _rootServiceProvider);
                    ConfigureMcpServer(services, serverArguments.Transport);
                })
                .Build();
        }
    }

    public IEnumerable<ArgumentDefinition<string>>? GetArgumentChain() => _arguments;

    public void ClearArgumentChain()
    {
        _arguments.Clear();
    }

    public void AddArgumentToChain(ArgumentDefinition<string> argument)
    {
        _arguments.Add(argument);
    }

    private static ArgumentDefinition<string> GetPortArgument()
    {
        var definition = ArgumentDefinitions.Service.Port;
        return new ArgumentDefinition<string>(definition.Name, definition.Description, definition.DefaultValue.ToString());
    }

    private static void ConfigureMcpServer(IServiceCollection services, String transport)
    {
        services.AddSingleton<ToolOperations>();

        services.AddSingleton<McpServerOptions>(provider =>
        {
            var toolOperations = provider.GetRequiredService<ToolOperations>();

            var entryAssembly = Assembly.GetEntryAssembly();
            var assemblyName = entryAssembly?.GetName();
            var serverName = entryAssembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Azure MCP Server";
            var mcpServerOptions = new McpServerOptions
            {
                ServerInfo = new Implementation
                {
                    Name = serverName,
                    Version = assemblyName?.Version?.ToString() ?? "1.0.0-beta"
                },
                Capabilities = new ServerCapabilities
                {
                    Tools = toolOperations.ToolsCapability
                },
                ProtocolVersion = "2024-11-05"
            };

            return mcpServerOptions;
        });
        services.AddSingleton(provider =>
        {
            var transport = provider.GetService<ITransport>();
            var options = provider.GetRequiredService<McpServerOptions>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

            if (transport == null)
            {
                return new AzureMcpServer(options, loggerFactory, provider);
            }
            else
            {
                return McpServerFactory.Create(transport, options, loggerFactory);
            }

        });

        if (transport != TransportTypes.Sse)
        {
            services.AddSingleton<ITransport>(provider =>
            {
                var options = provider.GetRequiredService<McpServerOptions>();

                return new StdioServerTransport(options,
                    provider.GetRequiredService<ILoggerFactory>());
            });

            services.AddHostedService<StdioMcpServerHostedService>();
        }
    }

    private static void ConfigureServices(IServiceCollection services, IServiceProvider rootServiceProvider)
    {
        services.AddMemoryCache();
        services.AddSingleton(rootServiceProvider.GetRequiredService<CommandFactory>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<ICacheService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<ISubscriptionService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<IStorageService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<ICosmosService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<IMonitorService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<IResourceGroupService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<IAppConfigService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<IExternalProcessService>());
    }

    private sealed class StdioMcpServerHostedService(IMcpServer session) : BackgroundService
    {
        /// <inheritdoc />
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => session.RunAsync(stoppingToken);
    }
}