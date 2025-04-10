// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Models;
using AzureMCP.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments;

public class GlobalArguments
{
    [JsonPropertyName(ArgumentDefinitions.Common.ResourceGroupName)]
    public string? ResourceGroup { get; set; }

    [JsonPropertyName(ArgumentDefinitions.Common.TenantName)]
    public string? Tenant { get; set; }

    [JsonPropertyName(ArgumentDefinitions.Common.AuthMethodName)]
    public AuthMethod? AuthMethod { get; set; }

    public RetryPolicyArguments? RetryPolicy { get; set; }
}