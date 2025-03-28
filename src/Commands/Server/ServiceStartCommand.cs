using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;
using AzureMCP.Arguments.Server;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Hosting;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;

namespace AzureMCP.Commands.Server;

public class ServiceStartCommand : ICommand
{
    private readonly Command _command;
    private readonly IServiceProvider _rootServiceProvider;
    private readonly List<ArgumentDefinition<string>> _arguments = [];
    private readonly Option<string> _transportOption;

    public ServiceStartCommand(IServiceProvider serviceProvider)
    {
        _arguments.Add(ArgumentDefinitions.Service.Transport);
        _transportOption = ArgumentDefinitions.Service.Transport.ToOption();
        _rootServiceProvider = serviceProvider;

        _command = new Command("start", "Starts Azure MCP server.");
        _command.AddOption(_transportOption);
        _command.SetHandler(parsingContext => ExecuteAsync(new CommandContext(_rootServiceProvider), parsingContext.ParseResult));
    }

    public async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult commandOptions)
    {
        var serverOptions = new ServerStartArguments
        {
            Transport = commandOptions.GetValueForOption(_transportOption) ?? TransportTypes.StdIo,
        };

        using var host = CreateHost(serverOptions);

        await host.StartAsync(CancellationToken.None);

        // Wait for the host to be terminated
        await host.WaitForShutdownAsync(CancellationToken.None);

        return context.Response;
    }

    public Command GetCommand() => _command;

    private IHost CreateHost(ServerStartArguments serverArguments)
    {
        if (serverArguments.Transport == TransportTypes.Sse)
        {
            var builder = WebApplication.CreateBuilder([]);

            ConfigureServices(builder.Services, _rootServiceProvider);
            ConfigureMcpServer(builder.Services, serverArguments.Transport);

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

    private static void ConfigureMcpServer(IServiceCollection services, String transport)
    {
        services.AddSingleton<ToolOperations>();

        services.AddSingleton<McpServerOptions>(provider =>
        {
            var toolOperations = provider.GetRequiredService<ToolOperations>();

            var entryAssembly = Assembly.GetEntryAssembly();
            var assemblyName = entryAssembly?.GetName();
            var serverName = entryAssembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Azure MCP server";
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
            var transport = provider.GetService<IServerTransport>();
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
            services.AddSingleton<IServerTransport>(provider =>
            {
                var options = provider.GetRequiredService<McpServerOptions>();

                return new StdioServerTransport(options.ServerInfo.Name,
                    provider.GetRequiredService<ILoggerFactory>());
            });
        }

        services.AddHostedService<McpServerHostedService>();
    }

    private static void ConfigureServices(IServiceCollection services, IServiceProvider rootServiceProvider)
    {
        services.AddSingleton(rootServiceProvider.GetRequiredService<CommandFactory>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<ICacheService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<ISubscriptionService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<IStorageService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<ICosmosService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<IMonitorService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<IResourceGroupService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<IAppConfigService>());
    }
}
