// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Cosmos;

public class BaseCosmosArguments : SubscriptionArguments
{
    [JsonPropertyName(ArgumentDefinitions.Cosmos.AccountName)]
    public string? Account { get; set; }
}