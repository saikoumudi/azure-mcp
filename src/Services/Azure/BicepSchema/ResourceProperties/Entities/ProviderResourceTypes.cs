// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace AzureMcp.Services.Azure.BicepSchema.ResourceProperties.Entities;

public record ProviderResourceTypes
{
    public ProviderResourceTypes(string provider)
    {
        Provider = provider;
        ResourceTypes = [];
    }

    [JsonPropertyName("provider")]
    public string Provider { get; init; }


    [JsonPropertyName("resourceTypes")]
    public Dictionary<string, UniqueResourceType> ResourceTypes { get; init; }
}
