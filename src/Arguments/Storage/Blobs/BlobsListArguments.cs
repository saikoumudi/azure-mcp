using System.Text.Json.Serialization;
using AzureMCP.Models;

namespace AzureMCP.Arguments.Storage.Blobs;

public class BlobsListArguments : BaseStorageArguments
{
    [JsonPropertyName(ArgumentDefinitions.Storage.ContainerName)]
    public string? Container { get; set; }
}
