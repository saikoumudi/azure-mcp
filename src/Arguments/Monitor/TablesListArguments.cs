using System.Text.Json.Serialization;
using AzureMCP.Models;
using AzureMCP.Arguments;

namespace AzureMCP.Arguments.Monitor;

public class TablesListArguments : BaseMonitorArguments
{
    [JsonPropertyName(ArgumentDefinitions.Monitor.WorkspaceNameName)]
    public string? WorkspaceName { get; set; }

     [JsonPropertyName(ArgumentDefinitions.Monitor.TableTypeName)]
    public string? TableType { get; set; }
}
