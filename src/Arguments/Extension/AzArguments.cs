using AzureMCP.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Extension;

public class AzArguments : BaseArguments
{
    [JsonPropertyName(ArgumentDefinitions.Extension.Az.CommandName)]
    public string? Command { get; set; }
}