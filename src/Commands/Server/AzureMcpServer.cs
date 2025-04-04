using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Messages;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;

namespace AzureMCP.Commands.Server;

/// <summary>
/// Wraps the implementation of an McpServer.  Used when the transport is not immediately available or active. 
/// (i.e. The user has not instantiated a connection to the MCP server.)
/// </summary>
public class AzureMcpServer(McpServerOptions serverOptions,
    ILoggerFactory? loggerFactory = null,
    IServiceProvider? serviceProvider = null) : IMcpServer
{
    private readonly ILoggerFactory? _loggerFactory = loggerFactory;
    private readonly IServiceProvider? _serviceProvider = serviceProvider;

    private IMcpServer? _implementation;

    public Boolean IsInitialized => _implementation != null;

    public ClientCapabilities? ClientCapabilities => _implementation?.ClientCapabilities;

    public Implementation? ClientInfo => _implementation?.ClientInfo;

    public McpServerOptions ServerOptions { get; } = serverOptions;

    public IServiceProvider? Services => _implementation?.Services;

    public void AddNotificationHandler(String method, Func<JsonRpcNotification, Task> handler)
    {
        if (_implementation is null)
        {
            throw new InvalidOperationException("Connect to the /sse endpoint before setting notification handlers.");
        }

        _implementation.AddNotificationHandler(method, handler);
    }

    /// <summary>
    /// Sets the transport layer and creates the underlying MCP server.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the MCP server has already been set and started.</exception>
    public async Task SetTransportAndStartAsync(ITransport transport, CancellationToken cancellationToken = default)
    {
        if (_implementation != null)
        {
            var oldTransport = _implementation;
            try
            {
                await oldTransport.DisposeAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // RequestAborted always triggers when the client disconnects before a complete response body is written,
                // but this is how SSE connections are typically closed.
            }
        }

        _implementation = McpServerFactory.Create(transport, ServerOptions, _loggerFactory, _serviceProvider);

        await StartAsync(cancellationToken);
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

    public Task<T> SendRequestAsync<T>(JsonRpcRequest request, CancellationToken cancellationToken)
        where T : class
    {
        if (_implementation is null)
        {
            throw new InvalidOperationException("Connect to the /sse endpoint before sending requests.");
        }

        return _implementation.SendRequestAsync<T>(request, cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_implementation is null)
        {
            return Task.CompletedTask;
        }

        return _implementation.IsInitialized
            ? Task.CompletedTask
            : _implementation.StartAsync(cancellationToken);
    }
}
