using AzureMCP.Models;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Cosmos;

public class BaseCosmosArguments : BaseArgumentsWithSubscription
{
    [JsonPropertyName(ArgumentDefinitions.Cosmos.AccountName)]
    public string? Account { get; set; }
}
