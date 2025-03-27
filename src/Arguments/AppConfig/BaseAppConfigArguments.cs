using AzureMCP.Models;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.AppConfig;

public class BaseAppConfigArguments : BaseArgumentsWithSubscriptionId
{
    [JsonPropertyName(ArgumentDefinitions.AppConfig.AccountName)]
    public string? Account { get; set; }
}