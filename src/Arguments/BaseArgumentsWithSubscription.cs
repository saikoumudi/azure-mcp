using AzureMCP.Models;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments;

public class BaseArgumentsWithSubscription : BaseArguments
{
    [JsonPropertyName(ArgumentDefinitions.Common.SubscriptionName)]
    public string? Subscription { get; set; }
}
