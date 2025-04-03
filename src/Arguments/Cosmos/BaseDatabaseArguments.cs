
using System.Text.Json.Serialization;
using AzureMCP.Models;

namespace AzureMCP.Arguments.Cosmos;

public class BaseDatabaseArguments : BaseCosmosArguments
{
    [JsonPropertyName(ArgumentDefinitions.Cosmos.DatabaseName)]
    public string? Database { get; set; }
}