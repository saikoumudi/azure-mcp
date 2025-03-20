using System.Text.Json.Serialization;
using AzureMCP.Models;

namespace AzureMCP.Models.Capabilities;

public class CommandCapability
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("fullPath")]
    public string FullPath { get; set; } = string.Empty;
    
    [JsonPropertyName("subcommands")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CommandCapability>? Subcommands { get; set; }
    
    [JsonPropertyName("args")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ArgumentInfo>? Arguments { get; set; }
} 