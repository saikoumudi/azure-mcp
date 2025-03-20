using System.Text.Json.Serialization;

namespace AzureMCP.Models;

public class ArgumentOption
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
