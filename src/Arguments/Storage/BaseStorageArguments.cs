// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Storage;

public class BaseStorageArguments : SubscriptionArguments
{
    [JsonPropertyName(ArgumentDefinitions.Storage.AccountName)]
    public string? Account { get; set; }
}