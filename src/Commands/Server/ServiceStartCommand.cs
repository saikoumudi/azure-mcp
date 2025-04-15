// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Arguments.Server;
using AzureMcp.Models;
using AzureMcp.Models.Argument;
using AzureMcp.Models.Command;
using AzureMcp.Services.Interfaces;
using AzureMcp.Services.Warmup;
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

namespace AzureMcp.Commands.Server;

[HiddenCommand]
public sealed class ServiceStartCommand(IServiceProvider serviceProvider) : BaseCommand
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly Option<string> _transportOption = ArgumentDefinitions.Service.Transport.ToOption();
    private readonly Option<int> _portOption = ArgumentDefinitions.Service.Port.ToOption();

    protected override string GetCommandName() => "start";

    protected override string GetCommandDescription() => "Starts Azure MCP Server.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_transportOption);
        command.AddOption(_portOption);
    }

    protected override void RegisterArguments()
    {
        base.RegisterArguments();
        AddArgument(GetTransportArgument());
        AddArgument(GetPortArgument());
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult commandOptions)
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
        await host.WaitForShutdownAsync(CancellationToken.None);

        return context.Response;
    }

    private IHost CreateHost(ServiceStartArguments serverArguments)
    {
        if (serverArguments.Transport == TransportTypes.Sse)
        {
            var builder = WebApplication.CreateBuilder([]);
            ConfigureServices(builder.Services, _serviceProvider);
            ConfigureMcpServer(builder.Services, serverArguments.Transport);

            builder.WebHost.ConfigureKestrel(server =>
            {
                server.ListenLocalhost(serverArguments.Port);
            });

            var application = builder.Build();

            application.MapMcp();

            return application;
        }
        else
        {
            return Host.CreateDefaultBuilder()
                .ConfigureLogging(logging => logging.ClearProviders())
                .ConfigureServices(services =>
                {
                    ConfigureServices(services, _serviceProvider);
                    ConfigureMcpServer(services, serverArguments.Transport);
                })
                .Build();
        }
    }

    private static ArgumentDefinition<string> GetTransportArgument()
    {
        var definition = ArgumentDefinitions.Service.Transport;
        return new ArgumentDefinition<string>(
            definition.Name,
            definition.Description,
            definition.DefaultValue?.ToString() ?? TransportTypes.StdIo);
    }

    private static ArgumentDefinition<string> GetPortArgument()
    {
        var definition = ArgumentDefinitions.Service.Port;
        return new ArgumentDefinition<string>(
            definition.Name,
            definition.Description,
            definition.DefaultValue.ToString());
    }

    private static void ConfigureMcpServer(IServiceCollection services, string transport)
    {
        services.AddSingleton<ToolOperations>();

        services.AddSingleton(provider =>
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
        services.AddHostedService<WarmupHostedService>();
    }

    private static void ConfigureServices(IServiceCollection services, IServiceProvider rootServiceProvider)
    {
        services.AddMemoryCache();
        services.AddSingleton(rootServiceProvider.GetRequiredService<CommandFactory>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<ICacheService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<ISubscriptionService>());
        services.AddSingleton(rootServiceProvider.GetRequiredService<ITenantService>());
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