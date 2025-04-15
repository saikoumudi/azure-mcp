// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Client;
using System.Text.Json;
using Xunit;

namespace AzureMCP.Tests.Commands.Client;

public class LiveClientTests : IClassFixture<McpClientFixture>
{
    private readonly IMcpClient _client;

    public LiveClientTests(McpClientFixture fixture)
    {
        _client = fixture.Client;
    }

    [LiveOnlyFact]
    public async Task Should_List_Tools()
    {
        var tools = await _client.ListToolsAsync();
        Assert.NotEmpty(tools);
    }

    [Fact]
    public async Task Client_Should_Invoke_Tool_Successfully()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _client.CallToolAsync(
            "azmcp-subscription-list",
            new Dictionary<string, object?>
            { });

        stopwatch.Stop();
        var elsec = stopwatch.ElapsedMilliseconds;
        var content = result.Content.FirstOrDefault(c => c.MimeType == "application/json")?.Text;

        Assert.False(string.IsNullOrWhiteSpace(content));

        var root = JsonSerializer.Deserialize<JsonElement>(content!);
        Assert.Equal(JsonValueKind.Object, root.ValueKind);

        Assert.True(root.TryGetProperty("subscriptions", out var subscriptionsArray));
        Assert.Equal(JsonValueKind.Array, subscriptionsArray.ValueKind);

        Assert.NotEmpty(subscriptionsArray.EnumerateArray());
    }

    [LiveOnlyFact]
    public async Task Client_Should_Handle_Invalid_Tools()
    {
        var result = await _client.CallToolAsync(
                "non_existent_tool",
                new Dictionary<string, object?>());

        var content = result.Content.FirstOrDefault(c => c.MimeType == "application/json")?.Text;
        Assert.True(string.IsNullOrWhiteSpace(content));
    }

    [LiveOnlyFact]
    public async Task Client_Should_Ping_Server_Successfully()
    {
        await _client.PingAsync();
        // If no exception is thrown, the ping was successful.
    }
}