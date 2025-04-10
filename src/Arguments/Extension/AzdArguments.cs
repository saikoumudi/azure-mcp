using AzureMCP.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Extension;

public class AzdArguments : GlobalArguments
{
    [JsonPropertyName(ArgumentDefinitions.Extension.Azd.CommandName)]
    public string? Command { get; set; }
}