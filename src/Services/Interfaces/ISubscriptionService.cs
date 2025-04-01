using Azure.ResourceManager.Resources;
using AzureMCP.Arguments;
using AzureMCP.Models;

namespace AzureMCP.Services.Interfaces;

public interface ISubscriptionService
{
    Task<List<ArgumentOption>> GetSubscriptions(string? tenantId = null, RetryPolicyArguments? retryPolicy = null);
    Task<SubscriptionResource> GetSubscription(string subscription, string? tenantId = null, RetryPolicyArguments? retryPolicy = null);
    bool IsSubscriptionId(string subscription, string? tenantId = null);
    Task<string> GetSubscriptionIdByName(string subscriptionName, string? tenantId = null, RetryPolicyArguments? retryPolicy = null);
    Task<string> GetSubscriptionNameById(string subscriptionId, string? tenantId = null, RetryPolicyArguments? retryPolicy = null);
}