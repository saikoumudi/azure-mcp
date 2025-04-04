
using AzureMCP.Models;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Cosmos;

public class BaseDatabaseArguments : BaseCosmosArguments
{
    [JsonPropertyName(ArgumentDefinitions.Cosmos.DatabaseName)]
    public string? Database { get; set; }
}