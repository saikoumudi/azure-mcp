using AzureMcp.Services.Interfaces;

namespace AzureMcp.Services.Azure.Datadog;

public class DatadogService : IDatadogService
{
    public async Task<List<string>> ListMonitoredResources(string resourceGroup, string subscription)
    {
        // Simulate fetching monitored resources from Datadog
        await Task.Delay(100); // Simulate async operation
        
        return new List<string> { "Resource1", "Resource2", "Resource3" };
    }
}