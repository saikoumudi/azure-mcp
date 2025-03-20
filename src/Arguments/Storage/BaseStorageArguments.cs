using System.Text.Json.Serialization;
using AzureMCP.Models;

namespace AzureMCP.Arguments.Storage;

public class BaseStorageArguments : BaseArgumentsWithSubscriptionId
{
    [JsonPropertyName(ArgumentDefinitions.Storage.AccountName)]
    public string? Account { get; set; }
}
