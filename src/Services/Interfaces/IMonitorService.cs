using AzureMCP.Arguments;
using AzureMCP.Models.Monitor;
using System.Text.Json;

namespace AzureMCP.Services.Interfaces;

public interface IMonitorService
{
    Task<List<JsonDocument>> QueryWorkspace(
        string subscription,
        string workspace,
        string query,
        int timeSpanDays = 1,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);

    Task<List<string>> ListTables(
        string subscription,
        string resourceGroup,
        string workspace,
        string? tableType = "CustomLog",
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);

    Task<List<WorkspaceInfo>> ListWorkspaces(
        string subscription,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);

    Task<object> QueryLogs(
        string subscription,
        string workspace,
        string query,
        string table,
        int? hours = 24,
        int? limit = 20,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);
}