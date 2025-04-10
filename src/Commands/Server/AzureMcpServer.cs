// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.Diagnostics;

namespace AzureMCP.Commands.Server;

/// <summary>
/// Wraps the implementation of an McpServer.  Used when the transport is not immediately available or active. 
/// (i.e. The user has not instantiated a connection to the MCP Server.)
/// </summary>
public class AzureMcpServer(McpServerOptions serverOptions,
    ILoggerFactory? loggerFactory = null,
    IServiceProvider? serviceProvider = null) : IMcpServer
{
    private readonly ILoggerFactory? _loggerFactory = loggerFactory;
    private readonly IServiceProvider? _serviceProvider = serviceProvider;

    private IMcpServer? _implementation;
    private ITransport? _implementationTransport;

    public Boolean IsInitialized => _implementation != null;

    public ClientCapabilities? ClientCapabilities => _implementation?.ClientCapabilities;

    public Implementation? ClientInfo => _implementation?.ClientInfo;

    public McpServerOptions ServerOptions => _implementation?.ServerOptions ?? serverOptions;

    public IServiceProvider? Services => _implementation?.Services;

    public LoggingLevel? LoggingLevel => _implementation?.LoggingLevel;

    /// <summary>
    /// Sets the transport layer and runs the underlying MCP server.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the MCP server has already been set and run.</exception>
    public async Task SetTransportAndRunAsync(ITransport transport, CancellationToken cancellationToken = default)
    {
        if (_implementation != null)
        {
            var oldServer = _implementation;
            try
            {
                await oldServer.DisposeAsync().ConfigureAwait(false);

                Debug.Assert(_implementationTransport is not null);
                await _implementationTransport.DisposeAsync();
            }
            catch (OperationCanceledException)
            {
                // RequestAborted always triggers when the client disconnects before a complete response body is written,
                // but this is how SSE connections are typically closed.
            }
        }

        _implementationTransport = transport;
        _implementation = McpServerFactory.Create(transport, ServerOptions, _loggerFactory, _serviceProvider);

        await RunAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _implementation is null ? ValueTask.CompletedTask : _implementation.DisposeAsync();
    }

    public Task SendMessageAsync(IJsonRpcMessage message, CancellationToken cancellationToken = default)
    {
        if (_implementation is null)
        {
            throw new InvalidOperationException("Connect to the /sse endpoint before setting notification handlers.");
        }

        return _implementation.SendMessageAsync(message, cancellationToken);
    }

    public Task<JsonRpcResponse> SendRequestAsync(JsonRpcRequest request, CancellationToken cancellationToken = default)
    {
        if (_implementation is null)
        {
            throw new InvalidOperationException("Connect to the /sse endpoint before sending requests.");
        }

        return _implementation.SendRequestAsync(request, cancellationToken);
    }

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (_implementation is null)
        {
            return Task.CompletedTask;
        }

        return _implementation.RunAsync(cancellationToken);
    }

    public IAsyncDisposable RegisterNotificationHandler(
        string method, Func<JsonRpcNotification, CancellationToken, Task> handler)
    {
        if (_implementation is null)
        {
            throw new InvalidOperationException(
                "Connect to the /sse endpoint before registering notification handlers. " +
                "Handlers may also be registered when constructing the server via McpServerOptions.");
        }

        return _implementation.RegisterNotificationHandler(method, handler);
    }
}