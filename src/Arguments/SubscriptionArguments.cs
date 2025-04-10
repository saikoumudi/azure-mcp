// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments;

public class SubscriptionArguments : GlobalArguments
{
    [JsonPropertyName(ArgumentDefinitions.Common.SubscriptionName)]
    public string? Subscription { get; set; }
}