using Azure.ResourceManager.Resources;
using AzureMCP.Arguments;
using AzureMCP.Models;

namespace AzureMCP.Services.Interfaces;

public interface ISubscriptionService
{
    Task<List<ArgumentOption>> GetSubscriptions(string? tenantId = null, RetryPolicyArguments? retryPolicy = null);
    Task<SubscriptionResource> GetSubscription(string subscriptionId, string? tenantId = null, RetryPolicyArguments? retryPolicy = null);
}