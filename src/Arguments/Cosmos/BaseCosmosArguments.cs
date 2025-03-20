using System.Text.Json.Serialization;
using AzureMCP.Models;

namespace AzureMCP.Arguments.Cosmos;

public class BaseCosmosArguments : BaseArgumentsWithSubscriptionId
{
    [JsonPropertyName(ArgumentDefinitions.Cosmos.AccountName)]
    public string? Account { get; set; }
}
