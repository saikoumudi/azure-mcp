using AzureMCP.Commands;
using AzureMCP.Commands.Server;
using McpDotNet.Protocol.Types;
using McpDotNet.Server;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace AzureMCP.Tests.Commands.Server
{
    public class McpStartServerCommandTests : IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMcpServer _mcpServer;

        public McpStartServerCommandTests()
        {
            _mcpServer = Substitute.For<IMcpServer>();

            var collection = new ServiceCollection();
            collection.AddSingleton(_mcpServer);

            _serviceProvider = collection.BuildServiceProvider();
        }

        [Fact]
        public async Task StartsServer()
        {
            var commandFactory = new CommandFactory(_serviceProvider);
            var service = new McpServerHostedService(_mcpServer, _serviceProvider, commandFactory);
            var parameters = new ListToolsRequestParams();
            var requestContext = new RequestContext<ListToolsRequestParams>(_mcpServer, parameters);

            await service.StartAsync(CancellationToken.None);

            var handler = _mcpServer.ListToolsHandler;
            Assert.NotNull(handler);
        }

        public ValueTask DisposeAsync()
        {
            return _mcpServer.DisposeAsync();
        }
    }
}
