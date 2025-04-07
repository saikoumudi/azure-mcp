
using AzureMCP.Arguments.Storage;
using AzureMCP.Models.Argument;
using System.Text.Json.Serialization;

public class BaseContainerArguments : BaseStorageArguments
{
    [JsonPropertyName(ArgumentDefinitions.Storage.ContainerName)]
    public string? Container { get; set; }
}