// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.AppConfig;

public class BaseAppConfigArguments : SubscriptionArguments
{
    [JsonPropertyName(ArgumentDefinitions.AppConfig.AccountName)]
    public string? Account { get; set; }
}