using AzureMCP.Models;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Monitor;

public class TableListArguments : BaseMonitorArguments
{
    [JsonPropertyName(ArgumentDefinitions.Monitor.WorkspaceNameName)]
    public string? WorkspaceName { get; set; }

    [JsonPropertyName(ArgumentDefinitions.Monitor.TableTypeName)]
    public string? TableType { get; set; }
}