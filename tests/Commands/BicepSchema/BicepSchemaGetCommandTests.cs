// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureMcp.Commands.BicepSchema;
using AzureMcp.Models.Command;
using AzureMcp.Services.Azure.BicepSchema;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AzureMcp.Tests.Commands.BicepSchema;

public class BicepSchemaGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBicepSchemaService _bicepSchemaService;
    private readonly ILogger<BicepSchemaGetCommand> _logger;
    private readonly CommandContext _context;
    private readonly BicepSchemaGetCommand _command;
    private readonly Parser _parser;

    public BicepSchemaGetCommandTests()
    {
        _bicepSchemaService = Substitute.For<IBicepSchemaService>();
        _logger = Substitute.For<ILogger<BicepSchemaGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_bicepSchemaService);

        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _command = new(_logger);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSchema_WhenResourceTypeExists()
    {
        var args = _parser.Parse([
        "--resourceType", "Microsoft.Sql/servers/databases/schemas",
        "--subscription", "knownSubscription"
        ]);

        var response = await _command.ExecuteAsync(_context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);


        //var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<BicepSchemaResult>(response.Results.ToString()!);

        Assert.Contains(
          "Microsoft.Sql/servers/databases/schemas@2021-11-01",
        "result.ToString()");
    }

    [Fact]
    public void ExecuteAsync_ReturnsError_WhenResourceTypeDoesNotExist()
    {
        var result = _bicepSchemaService.GetResourceTypeDefinitions(
            _serviceProvider, "Microsoft.Unknown/virtualReshatot");
        string response = SchemaGenerator.GetResponse(result, compactFormat: true);

        Assert.Contains("Resource type Microsoft.Unknown/virtualReshatot not found.", response);
    }

    private class BicepSchemaResult
    {
        [JsonPropertyName("blobs")]
        public List<string> Blobs { get; set; } = new();
    }
}
