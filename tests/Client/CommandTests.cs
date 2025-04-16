// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Types;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace AzureMcp.Tests.Client;

public class CommandTests : IClassFixture<McpClientFixture>
{
    private readonly IMcpClient _client;
    private readonly ITestOutputHelper _output;
    private LiveTestSettings? _testSettings;

    public CommandTests(McpClientFixture fixture, ITestOutputHelper output)
    {
        _client = fixture.Client;
        _output = output;
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_storage_accounts_by_subscription_id()
    {
        var settings = await GetTestSettingsAsync();

        var result = await CallToolAsync(
            "azmcp-storage-account-list",
            new()
            {
                { "subscription", settings.SubscriptionId }
            });

        JsonElement root = DeserializeJsonContent(result);

        Assert.True(root.TryGetProperty("accounts", out var accounts));
        Assert.Equal(JsonValueKind.Array, accounts.ValueKind);

        Assert.NotEmpty(accounts.EnumerateArray());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_storage_accounts_by_subscription_name()
    {
        var settings = await GetTestSettingsAsync();
        var result = await CallToolAsync(
            "azmcp-storage-account-list",
            new()
            {
                { "subscription", settings.SubscriptionName }
            });

        JsonElement root = DeserializeJsonContent(result);

        Assert.True(root.TryGetProperty("accounts", out var accounts));
        Assert.Equal(JsonValueKind.Array, accounts.ValueKind);

        Assert.NotEmpty(accounts.EnumerateArray());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_storage_accounts_by_subscription_name_with_tenant()
    {
        var settings = await GetTestSettingsAsync();
        var result = await CallToolAsync(
            "azmcp-storage-account-list",
            new()
            {
                { "subscription", settings.SubscriptionName },
                { "tenant", settings.TenantId }
            });

        JsonElement root = DeserializeJsonContent(result);

        Assert.True(root.TryGetProperty("accounts", out var accounts));
        Assert.Equal(JsonValueKind.Array, accounts.ValueKind);

        Assert.NotEmpty(accounts.EnumerateArray());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_storage_accounts_by_subscription_name_with_tenant_name()
    {
        var settings = await GetTestSettingsAsync();
        var result = await CallToolAsync(
            "azmcp-storage-account-list",
            new()
            {
                { "subscription", settings.SubscriptionName },
                { "tenant", settings.TenantName }
            });

        JsonElement root = DeserializeJsonContent(result);

        Assert.True(root.TryGetProperty("accounts", out var accounts));
        Assert.Equal(JsonValueKind.Array, accounts.ValueKind);

        Assert.NotEmpty(accounts.EnumerateArray());
    }

    private async Task<LiveTestSettings> GetTestSettingsAsync()
    {
        if (_testSettings != null)
        {
            return _testSettings;
        }
        
        var testSettingsFileName = ".testsettings.json";
        var directory = Path.GetDirectoryName(typeof(CommandTests).Assembly.Location);
        while(!string.IsNullOrEmpty(directory))
        {
            var testSettingsFilePath = Path.Combine(directory, testSettingsFileName);
            if (File.Exists(testSettingsFilePath))
            {
                var content = await File.ReadAllTextAsync(testSettingsFilePath);
                _testSettings = JsonSerializer.Deserialize<LiveTestSettings>(content);
                return _testSettings!;
            }

            directory = Path.GetDirectoryName(directory);
        }

        throw new FileNotFoundException($"Test settings file '{testSettingsFileName}' not found in the assembly directory or its parent directories.");
    }

    private Task<CallToolResponse> CallToolAsync(string command, Dictionary<string, object?> parameters)
    {
        return _client.CallToolAsync(command, parameters);
    }

    private JsonElement DeserializeJsonContent(CallToolResponse result)
    {
        var content = result.Content.FirstOrDefault(c => c.MimeType == "application/json")?.Text;
        Assert.False(string.IsNullOrWhiteSpace(content));

        _output.WriteLine(content);

        var root = JsonSerializer.Deserialize<JsonElement>(content!);
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        return root;
    }
}
