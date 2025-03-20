using Azure.Monitor.Query;
using Azure.ResourceManager.OperationalInsights;
using Azure.Core;
using Azure;
using System.Text.Json;
using AzureMCP.Services.Interfaces;
using AzureMCP.Arguments;
using AzureMCP.Models.Monitor;

namespace AzureMCP.Services.Azure.Monitor;

public class MonitorService : BaseAzureService, IMonitorService
{
    private const string ListTablesQuery = @"
        search *
        | distinct $table
        | sort by $table asc
    ";

    private const string TablePlaceholder = "{tableName}";

    private static readonly Dictionary<string, string> PredefinedQueries = new Dictionary<string, string>
    {
        ["recent"] = @"
            {tableName}
            | order by TimeGenerated desc
        ",
        ["errors"] = @"
            {tableName}
            | where level == ""ERROR""
            | order by TimeGenerated desc
        ",
        ["summary"] = @"
            {tableName}
            | summarize count() by level, bin(TimeGenerated, 1h)
            | order by TimeGenerated desc
        "
    };

    public async Task<List<JsonDocument>> QueryWorkspace(
        string workspaceId,
        string query,
        int timeSpanDays = 1,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null)
    {
        if (string.IsNullOrEmpty(workspaceId))
            throw new ArgumentException("Workspace ID cannot be null or empty", nameof(workspaceId));
        if (string.IsNullOrEmpty(query))
            throw new ArgumentException("Query cannot be null or empty", nameof(query));

        var credential = GetCredential(tenantId);
        var options = new LogsQueryClientOptions();
        if (retryPolicy != null)
        {
            options.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
            options.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
            options.Retry.MaxRetries = retryPolicy.MaxRetries;
            options.Retry.Mode = retryPolicy.Mode;
            options.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
        }
        var client = new LogsQueryClient(credential, options);

        try
        {
            var response = await client.QueryWorkspaceAsync(
                workspaceId,
                query,
                new QueryTimeRange(TimeSpan.FromDays(timeSpanDays))
            );

            var results = new List<JsonDocument>();
            if (response.Value.Table != null)
            {
                var rows = response.Value.Table.Rows;
                var columns = response.Value.Table.Columns;

                if (rows != null && columns != null && rows.Any())
                {
                    foreach (var row in rows)
                    {
                        var rowDict = new Dictionary<string, object>();
                        for (int i = 0; i < columns.Count; i++)
                        {
                            rowDict[columns[i].Name] = row[i];
                        }
                        results.Add(JsonDocument.Parse(JsonSerializer.Serialize(rowDict)));
                    }
                }
            }
            return results;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error querying workspace: {ex.Message}", ex);
        }
    }

    public async Task<List<string>> ListTables(
        string subscriptionId,
        string resourceGroup,
        string workspaceName,
        string? tableType = "CustomLog",
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            throw new ArgumentException("Subscription ID cannot be null or empty", nameof(subscriptionId));
        if (string.IsNullOrEmpty(resourceGroup))
            throw new ArgumentException("Resource group cannot be null or empty", nameof(resourceGroup));
        if (string.IsNullOrEmpty(workspaceName))
            throw new ArgumentException("Workspace name cannot be null or empty", nameof(workspaceName));

        try
        {
            var armClient = CreateArmClient(tenantId, retryPolicy);
            var subscription = await armClient.GetSubscriptionResource(
                new ResourceIdentifier($"/subscriptions/{subscriptionId}"))
                .GetAsync()
                .ConfigureAwait(false);

            var resourceGroupResource = await subscription.Value.GetResourceGroups()
                .GetAsync(resourceGroup)
                .ConfigureAwait(false);

            if (resourceGroupResource == null || resourceGroupResource.Value == null)
            {
                throw new Exception($"Resource group {resourceGroup} not found in subscription {subscriptionId}");
            }

            var workspace = await resourceGroupResource.Value.GetOperationalInsightsWorkspaceAsync(workspaceName)
                .ConfigureAwait(false);
            
            if (workspace == null || workspace.Value == null)
            {
                throw new Exception($"Workspace {workspaceName} not found in resource group {resourceGroup}");
            }

            var tableOperations = workspace.Value.GetOperationalInsightsTables();
            var tables = await tableOperations.GetAllAsync()
                .ToListAsync()
                .ConfigureAwait(false);
            
            return [.. tables
                .Where(table => string.IsNullOrEmpty(tableType) || table.Data.Schema.TableType.ToString() == tableType)
                .Select(table => table.Data.Name)
                .OrderBy(name => name)];
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing tables: {ex.Message}", ex);
        }
    }

    public async Task<List<WorkspaceInfo>> ListWorkspaces(string subscriptionId, string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null)
    {
        ArgumentNullException.ThrowIfNull(subscriptionId);

        var armClient = CreateArmClient(tenantId, retryPolicy);
        var subscriptionResource = new ResourceIdentifier($"/subscriptions/{subscriptionId}");

        try
        {
            var subscription = await armClient
                .GetSubscriptionResource(subscriptionResource)
                .GetAsync()
                .ConfigureAwait(false);

            var workspaces = await subscription.Value
                .GetOperationalInsightsWorkspacesAsync()
                .Select(workspace => new WorkspaceInfo
                {
                    Name = workspace.Data.Name,
                    CustomerId = workspace.Data.CustomerId?.ToString() ?? string.Empty,
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return workspaces;
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new Exception($"Error retrieving Log Analytics workspaces: {ex.Message}", ex);
        }
    }

    public async Task<object> QueryLogs(
        string workspaceId,
        string query,
        string table,
        int? hours = 24,
        int? limit = 20,
        string? subscriptionId = null,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null)
    {
        // Validate required parameters
        if (string.IsNullOrEmpty(workspaceId))
            throw new ArgumentException("Workspace ID cannot be null or empty", nameof(workspaceId));
        if (string.IsNullOrEmpty(table))
            throw new ArgumentException("Table name cannot be null or empty", nameof(table));

        // Check if the query is a predefined query name
        if (!string.IsNullOrEmpty(query) && PredefinedQueries.ContainsKey(query.Trim().ToLower()))
        {
            query = PredefinedQueries[query.Trim().ToLower()];
            // Replace table placeholder with actual table name
            query = query.Replace(TablePlaceholder, table);
        }

        if (string.IsNullOrEmpty(query))
            throw new ArgumentException("Query cannot be null or empty", nameof(query));

        // Add limit to query if specified and not already present
        if (limit.HasValue && !query.Contains("limit", StringComparison.CurrentCultureIgnoreCase))
        {
            query = $"{query}\n| limit {limit}";
        }

        try
        {
            // Convert hours to days for QueryWorkspace
            double days = (hours ?? 24) / 24.0;

            // Call QueryWorkspace with the prepared query
            var jsonResults = await QueryWorkspace(
                workspaceId,
                query,
                (int)Math.Ceiling(days), // Round up to ensure we cover the full time range
                tenantId,
                retryPolicy
            );

            // Convert JsonDocument results to Dictionary<string, object>
            var results = new List<Dictionary<string, object?>>();
            foreach (var jsonDoc in jsonResults)
            {
                var dict = new Dictionary<string, object?>();
                foreach (var property in jsonDoc.RootElement.EnumerateObject())
                {
                    dict[property.Name] = GetValueFromJsonElement(property.Value);
                }
                results.Add(dict);
            }

            // Return the list as an object to match the interface
            return results;
        }
        catch (Exception ex)
        {
            // Provide a more specific error message based on the exception type
            string errorMessage = ex switch
            {
                RequestFailedException rfe => $"Azure request failed: {rfe.Status} - {rfe.Message}",
                TimeoutException => "The query timed out. Try simplifying your query or reducing the time range.",
                _ => $"Error querying logs: {ex.Message}"
            };

            throw new Exception(errorMessage, ex);
        }
    }

    // Helper method to convert JsonElement to appropriate .NET types
    private static object? GetValueFromJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString() ?? string.Empty; // Return empty string instead of null
            case JsonValueKind.Number:
                if (element.TryGetInt64(out long longValue))
                    return longValue;
                return element.GetDouble();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.Object:
                var obj = new Dictionary<string, object?>();
                foreach (var property in element.EnumerateObject())
                {
                    obj[property.Name] = GetValueFromJsonElement(property.Value);
                }
                return obj;
            case JsonValueKind.Array:
                var array = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    array.Add(GetValueFromJsonElement(item));
                }
                return array;
            default:
                return null;
        }
    }
}
