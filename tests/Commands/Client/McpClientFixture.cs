// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using Xunit;

namespace AzureMCP.Tests.Commands.Client;

public class McpClientFixture : IAsyncLifetime
{
    public IMcpClient Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var azMcpPath = Environment.GetEnvironmentVariable("AZURE_MCP_PATH");

        if (true)
        {
            var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "Test Server",
                Command = "C:\\Users\\vigera\\Documents\\mcp\\azure-mcp\\src\\.dist\\azmcp.exe",
                Arguments = new[] { "server", "start" },
            });

            Client = await McpClientFactory.CreateAsync(clientTransport);
        }
    }

    public async Task DisposeAsync()
    {
        if (Client is IAsyncDisposable disposable)
            await disposable.DisposeAsync();
    }
}