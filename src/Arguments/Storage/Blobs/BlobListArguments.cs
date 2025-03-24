using AzureMCP.Models;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Storage.Blob;

public class BlobListArguments : BaseStorageArguments
{
    [JsonPropertyName(ArgumentDefinitions.Storage.ContainerName)]
    public string? Container { get; set; }
}
