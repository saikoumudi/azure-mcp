using Azure.ResourceManager.Resources;
using AzureMCP.Arguments;
using AzureMCP.Models.ResourceGroup;

namespace AzureMCP.Services.Interfaces;

public interface IResourceGroupService
{
    Task<List<ResourceGroupInfo>> GetResourceGroups(string subscriptionId, string? tenant = null, RetryPolicyArguments? retryPolicy = null);
    Task<ResourceGroupInfo?> GetResourceGroup(string subscriptionId, string resourceGroupName, string? tenant = null, RetryPolicyArguments? retryPolicy = null);
    Task<ResourceGroupResource?> GetResourceGroupResource(string subscriptionId, string resourceGroupName, string? tenant = null, RetryPolicyArguments? retryPolicy = null);
}