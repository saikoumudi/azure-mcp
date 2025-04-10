// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMCP.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.AppConfig.KeyValue;

public class KeyValueSetArguments : BaseKeyValueArguments
{
    [JsonPropertyName(ArgumentDefinitions.AppConfig.ValueName)]
    public string? Value { get; set; }
}