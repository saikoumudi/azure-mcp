using AzureMCP.Models;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Storage;

public class BaseStorageArguments : BaseArgumentsWithSubscriptionId
{
    [JsonPropertyName(ArgumentDefinitions.Storage.AccountName)]
    public string? Account { get; set; }
}
