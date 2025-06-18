// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Bicep.Types;
using Azure.Bicep.Types.Az;
using AzureMcp.Services.Azure.BicepSchema.ResourceProperties;
using AzureMcp.Services.Azure.BicepSchema.ResourceProperties.Entities;
using AzureMcp.Services.Azure.BicepSchema.Support;
using Microsoft.Extensions.DependencyInjection;

namespace AzureMcp.Services.Azure.BicepSchema;
public static class SchemaGenerator
{
    public static string GetResponse(TypesDefinitionResult typesDefinitionResult, bool compactFormat)
    {
        var allComplexTypes = new List<ComplexType>();
        allComplexTypes.AddRange(typesDefinitionResult.ResourceTypeEntities);
        allComplexTypes.AddRange(typesDefinitionResult.ResourceFunctionTypeEntities);
        allComplexTypes.AddRange(typesDefinitionResult.OtherComplexTypeEntities);

#pragma warning disable IL2026
#pragma warning disable IL3050
        string serialized = JsonSerializer.Serialize(
            allComplexTypes,
            new JsonSerializerOptions
            {
                WriteIndented = !compactFormat
            });
#pragma warning restore IL3050
#pragma warning restore IL2026
        return serialized;
    }

    public static TypesDefinitionResult GetResourceTypeDefinitions(
        IServiceProvider serviceProvider,
        string resourceTypeName,
        string? apiVersion = null)
    {
        ResourceVisitor resourceVisitor = serviceProvider.GetRequiredService<ResourceVisitor>();

        if (string.IsNullOrEmpty(apiVersion))
        {
            apiVersion = ApiVersionSelector.SelectLatestStable(resourceVisitor.GetResourceApiVersions(resourceTypeName));
        }

        return resourceVisitor.LoadSingleResource(resourceTypeName, apiVersion);
    }

    public static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<ITypeLoader, AzTypeLoader>();
        services.AddSingleton<ResourceVisitor>();
    }
}
