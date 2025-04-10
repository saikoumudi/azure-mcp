// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace AzureMCP.Commands.Server.Tests;

public class AzureMcpServerTests
{
    private readonly McpServerOptions _options = new() { ServerInfo = new() { Name = "test", Version = "1.0.0-beta" } };
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly ITransport _transport = Substitute.For<ITransport>();

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

        await wrapper.SetTransportAndRunAsync(_transport);

        Assert.True(wrapper.IsInitialized);
    }

    [Fact]
    public async Task SetTransportAndStartAsync_DisposesOldTransport()
    {
        var wrapper = new AzureMcpServer(_options, _loggerFactory);
        await wrapper.SetTransportAndRunAsync(_transport);

        var transport2 = Substitute.For<ITransport>();
        await wrapper.SetTransportAndRunAsync(transport2);

        await _transport.Received().DisposeAsync();
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
            () => wrapper.SendRequestAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task DisposeAsync_CompletesWhenNotInitialized()
    {
        var wrapper = new AzureMcpServer(_options);
        await wrapper.DisposeAsync();

        Assert.False(wrapper.IsInitialized);
    }
}