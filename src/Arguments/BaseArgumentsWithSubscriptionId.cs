using Azure.Core;
using AzureMCP.Models;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments;

public class BaseArgumentsWithSubscriptionId : BaseArguments
{
    [JsonPropertyName(ArgumentDefinitions.Common.SubscriptionIdName)]
    public string? SubscriptionId { get; set; }
}
