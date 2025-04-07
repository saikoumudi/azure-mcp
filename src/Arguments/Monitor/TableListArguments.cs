using AzureMCP.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMCP.Arguments.Monitor;

public class TableListArguments : BaseMonitorArguments, IWorkspaceArguments
{
    [JsonPropertyName(ArgumentDefinitions.Monitor.WorkspaceIdOrName)]
    public string? Workspace { get; set; }

    [JsonPropertyName(ArgumentDefinitions.Monitor.TableTypeName)]
    public string? TableType { get; set; }
}