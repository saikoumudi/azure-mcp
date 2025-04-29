namespace AzureMcp.Services.Interfaces;

public interface IDatadogService
{
    /// <summary>
    /// Lists monitored resources in Datadog.
    /// </summary>
    /// <param name="resourceGroup">The resource group name.</param>
    /// <param name="subscription">The subscription ID or name.</param>
    /// <returns>List of monitored resources.</returns>
    Task<List<string>> ListMonitoredResources(string resourceGroup, string subscription);
}