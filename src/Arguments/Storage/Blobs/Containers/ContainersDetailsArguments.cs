using System.Text.Json.Serialization;
using AzureMCP.Models;

namespace AzureMCP.Arguments.Storage.Blobs.Containers;

public class ContainersDetailsArguments : BaseStorageArguments
{
    [JsonPropertyName(ArgumentDefinitions.Storage.ContainerName)]
    public string? Container { get; set; }
}
