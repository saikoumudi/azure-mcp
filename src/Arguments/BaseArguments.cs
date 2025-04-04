using AzureMCP.Models;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments;

public class BaseArguments
{
    [JsonPropertyName(ArgumentDefinitions.Common.ResourceGroupName)]
    public string? ResourceGroup { get; set; }

    [JsonPropertyName(ArgumentDefinitions.Common.TenantIdName)]
    public string? TenantId { get; set; }

    [JsonPropertyName(ArgumentDefinitions.Common.AuthMethodName)]
    public AuthMethod? AuthMethod { get; set; }

    public RetryPolicyArguments? RetryPolicy { get; set; }
}