using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace AzureMCP.Commands.Server.Tests;

public class AzureMcpServerTests
{
    private readonly McpServerOptions _options;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ITransport _transport;

    public AzureMcpServerTests()
    {
        _options = new McpServerOptions { ServerInfo = new Implementation { Name = "test", Version = "1.0.0-beta" } };
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _transport = Substitute.For<ITransport>();
    }

    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        var wrapper = new AzureMcpServer(_options, _loggerFactory);

        Assert.False(wrapper.IsInitialized);
        Assert.Equal(_options, wrapper.ServerOptions);
        Assert.Null(wrapper.ClientCapabilities);
        Assert.Null(wrapper.ClientInfo);
    }

    [Fact]
    public async Task SetTransportAndStartAsync_InitializesServer()
    {
        var wrapper = new AzureMcpServer(_options, _loggerFactory);
        
        await wrapper.SetTransportAndStartAsync(_transport);

        Assert.True(wrapper.IsInitialized);
    }

    [Fact]
    public async Task SetTransportAndStartAsync_ThrowsWhenCalledTwice()
    {
        var wrapper = new AzureMcpServer(_options, _loggerFactory);
        await wrapper.SetTransportAndStartAsync(_transport);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => wrapper.SetTransportAndStartAsync(_transport));
    }

    [Fact]
    public async Task SendMessageAsync_ThrowsWhenNotInitialized()
    {
        var wrapper = new AzureMcpServer(_options);
        var message = new JsonRpcNotification { Method = "test" };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => wrapper.SendMessageAsync(message));
    }

    [Fact]
    public async Task SendRequestAsync_ThrowsWhenNotInitialized()
    {
        var wrapper = new AzureMcpServer(_options);
        var request = new JsonRpcRequest { Method = "test" };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => wrapper.SendRequestAsync<object>(request, CancellationToken.None));
    }

    [Fact]
    public void AddNotificationHandler_ThrowsWhenNotInitialized()
    {
        var wrapper = new AzureMcpServer(_options);

        Assert.Throws<InvalidOperationException>(
            () => wrapper.AddNotificationHandler("test", _ => Task.CompletedTask));
    }

    [Fact]
    public async Task DisposeAsync_CompletesWhenNotInitialized()
    {
        var wrapper = new AzureMcpServer(_options);
        await wrapper.DisposeAsync();

        Assert.False(wrapper.IsInitialized);
    }
}
