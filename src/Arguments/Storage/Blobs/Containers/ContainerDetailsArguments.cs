using System.Text.Json.Serialization;
using AzureMCP.Models;

namespace AzureMCP.Arguments.Storage.Blob.Container;

public class ContainerDetailsArguments : BaseStorageArguments
{
    [JsonPropertyName(ArgumentDefinitions.Storage.ContainerName)]
    public string? Container { get; set; }
}
