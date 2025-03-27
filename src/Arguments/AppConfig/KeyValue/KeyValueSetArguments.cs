using AzureMCP.Models;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.AppConfig.KeyValue;

public class KeyValueSetArguments : BaseAppConfigArguments
{
    [JsonPropertyName(ArgumentDefinitions.AppConfig.KeyName)]
    public string? Key { get; set; }

    [JsonPropertyName(ArgumentDefinitions.AppConfig.ValueName)]
    public string? Value { get; set; }

    [JsonPropertyName(ArgumentDefinitions.AppConfig.LabelName)]
    public string? Label { get; set; }
}