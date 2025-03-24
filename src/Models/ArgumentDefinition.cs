using System.CommandLine;
using System.Text.Json.Serialization;

namespace AzureMCP.Models;

public class ArgumentDefinition<T>(string name, string description, string? value = "", string? command = "", T? defaultValue = default, List<ArgumentOption>? values = null, bool required = false)
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = name;

    [JsonPropertyName("description")]
    public string Description { get; set; } = description;

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Value { get; set; } = value!;

    [JsonPropertyName("command")]
    public string Command { get; set; } = command!;

    [JsonPropertyName("default")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? DefaultValue { get; set; } = defaultValue;

    [JsonPropertyName("values")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ArgumentOption>? Values { get; set; } = values;

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Type { get; set; } = typeof(T).Name.ToLowerInvariant();

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Required { get; set; } = required;

    public bool ShouldSerializeValues() => Values?.Count > 0;

    public Option<T> ToOption()
    {
        return DefaultValue != null
            ? new Option<T>($"--{Name}", () => DefaultValue, Description)
            : new Option<T>($"--{Name}", Description);
    }

    public JsonPropertyNameAttribute ToJsonAttribute()
    {
        return new JsonPropertyNameAttribute(Name);
    }
}