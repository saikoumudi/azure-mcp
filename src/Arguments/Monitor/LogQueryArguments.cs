using System.Text.Json.Serialization;
using AzureMCP.Models;

namespace AzureMCP.Arguments.Monitor;

public class LogQueryArguments : BaseMonitorArguments
{
    [JsonPropertyName(ArgumentDefinitions.Monitor.WorkspaceIdName)]
    public string? WorkspaceId { get; set; }
    public string? Query { get; set; }
    public int? Hours { get; set; }
    public int? Limit { get; set; }
    public string? Table { get; set; }
}