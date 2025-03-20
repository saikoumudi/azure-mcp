using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using McpDotNet.Protocol.Transport;
using McpDotNet.Protocol.Types;
using McpDotNet.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AzureMCP.Commands.Server
{
    public class McpStartServerCommand : ICommand
    {
        private readonly Command _command;
        private readonly IServiceProvider _rootServiceProvider;

        public McpStartServerCommand(IServiceProvider serviceProvider)
        {
            _command = new Command("start", "Starts MCP server.");
            _command.SetHandler(parsingContext => InvokeAsync(parsingContext, CancellationToken.None));
            _rootServiceProvider = serviceProvider;
        }

        public Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult commandOptions) => throw new NotImplementedException("Not used.");

        public Command GetCommand() => _command;

        private async Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken)
        {
            // Create host
            using var host = CreateHostBuilder().Build();

            // Start the host
            await host.StartAsync(cancellationToken);

            // Wait for the host to be terminated
            await host.WaitForShutdownAsync(cancellationToken);

            return 0;
        }

        private IHostBuilder CreateHostBuilder()
        {
            var assemblyName = Assembly.GetCallingAssembly().GetName();
            var serverName = assemblyName?.Name ?? "Azure MCP server";
            var mcpServerOptions = new McpServerOptions
            {
                ServerInfo = new Implementation
                {
                    Name = serverName,
                    Version = assemblyName?.Version?.ToString() ?? "1.0.0-beta"
                },
                Capabilities = new ServerCapabilities
                {
                    Tools = new(),
                },
                ProtocolVersion = "2024-11-05"
            };

            var commandFactory = _rootServiceProvider.GetRequiredService<CommandFactory>();

            return Host.CreateDefaultBuilder()
                .ConfigureLogging(logging => logging.ClearProviders())
                .ConfigureServices(services =>
                {
                    services.AddSingleton(mcpServerOptions);
                    services.AddSingleton(commandFactory);
                    services.AddSingleton(provider =>
                        _rootServiceProvider.GetRequiredService<ISubscriptionService>());
                    services.AddSingleton(provider =>
                        _rootServiceProvider.GetRequiredService<IStorageService>());
                    services.AddSingleton(provider =>
                        _rootServiceProvider.GetRequiredService<ICosmosService>());
                    services.AddSingleton(provider =>
                        _rootServiceProvider.GetRequiredService<IMonitorService>());
                    services.AddSingleton(provider =>
                                            _rootServiceProvider.GetRequiredService<IResourceGroupService>());
                    services.AddSingleton<IServerTransport>(provider =>
                        new StdioServerTransport(serverName, provider.GetRequiredService<ILoggerFactory>()));

                    services.AddHostedService<McpServerHostedService>();
                    services.AddSingleton<IMcpServer>(provider =>
                    {
                        var transport = provider.GetRequiredService<IServerTransport>();
                        var options = provider.GetRequiredService<McpServerOptions>();
                        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                        var mcpServerFactory = new McpServerFactory(transport, options, loggerFactory);

                        return mcpServerFactory.CreateServer();
                    });
                });
        }

        public IEnumerable<ArgumentDefinition<string>>? GetArgumentChain() => Enumerable.Empty<ArgumentDefinition<string>>();

        public void ClearArgumentChain()
        {
            throw new NotImplementedException();
        }

        public void AddArgumentToChain(ArgumentDefinition<string> argument)
        {
            throw new NotImplementedException();
        }
    }
}
