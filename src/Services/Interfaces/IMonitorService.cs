using AzureMCP.Arguments;
using AzureMCP.Models.Monitor;
using System.Text.Json;

namespace AzureMCP.Services.Interfaces;

public interface IMonitorService
{
    Task<List<JsonDocument>> QueryWorkspace(
        string workspaceId,
        string query,
        int timeSpanDays = 1,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);

    Task<List<string>> ListTables(
        string subscription,
        string resourceGroup,
        string workspaceName,
        string? tableType = "CustomLog",
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);

    Task<List<WorkspaceInfo>> ListWorkspaces(
        string subscription,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);

    Task<object> QueryLogs(
        string workspaceId,
        string query,
        string table,
        int? hours = 24,
        int? limit = 20,
        string? subscription = null,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);
}