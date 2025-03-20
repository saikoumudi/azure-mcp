using System.Text.Json.Serialization;
using AzureMCP.Arguments;
using AzureMCP.Models;

namespace AzureMCP.Arguments.Monitor;

public class LogsQueryArguments : BaseMonitorArguments
{
    [JsonPropertyName(ArgumentDefinitions.Monitor.WorkspaceIdName)]
    public string? WorkspaceId { get; set; }
    public string? Query { get; set; }
    public int? Hours { get; set; }
    public int? Limit { get; set; }
    public string? Table { get; set; }
}