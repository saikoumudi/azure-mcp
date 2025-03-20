using AzureMCP.Models;
using AzureMCP.Arguments;
using AzureMCP.Models.ResourceGroup;

namespace AzureMCP.Services.Interfaces;

public interface IResourceGroupService
{
    Task<List<ResourceGroupInfo>> GetResourceGroups(string subscriptionId, string? tenantId = null, RetryPolicyArguments? retryPolicy = null);
}