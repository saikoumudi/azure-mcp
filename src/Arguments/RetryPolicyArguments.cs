using Azure.Core;
using AzureMCP.Models;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments;

/// <summary>
/// Represents retry policy configuration for Azure SDK clients
/// </summary>
public class RetryPolicyArguments
{
    [JsonPropertyName(ArgumentDefinitions.RetryPolicy.DelayName)]
    public double DelaySeconds { get; set; }

    [JsonPropertyName(ArgumentDefinitions.RetryPolicy.MaxDelayName)]
    public double MaxDelaySeconds { get; set; }

    [JsonPropertyName(ArgumentDefinitions.RetryPolicy.MaxRetriesName)]
    public int MaxRetries { get; set; }

    [JsonPropertyName(ArgumentDefinitions.RetryPolicy.ModeName)]
    public RetryMode Mode { get; set; }

    [JsonPropertyName(ArgumentDefinitions.RetryPolicy.NetworkTimeoutName)]
    public double NetworkTimeoutSeconds { get; set; }
}