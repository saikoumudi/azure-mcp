
using System.Text.Json.Serialization;
using AzureMCP.Arguments.Storage;
using AzureMCP.Models;

public class BaseContainerArguments : BaseStorageArguments
{
    [JsonPropertyName(ArgumentDefinitions.Storage.ContainerName)]
    public string? Container { get; set; }
}