using Azure.ResourceManager.Resources;
using AzureMCP.Arguments;
using AzureMCP.Models.Argument;

namespace AzureMCP.Services.Interfaces;

public interface ISubscriptionService
{
    Task<List<ArgumentOption>> GetSubscriptions(string? tenant = null, RetryPolicyArguments? retryPolicy = null);
    Task<SubscriptionResource> GetSubscription(string subscription, string? tenant = null, RetryPolicyArguments? retryPolicy = null);
    bool IsSubscriptionId(string subscription, string? tenant = null);
    Task<string> GetSubscriptionIdByName(string subscriptionName, string? tenant = null, RetryPolicyArguments? retryPolicy = null);
    Task<string> GetSubscriptionNameById(string subscriptionId, string? tenant = null, RetryPolicyArguments? retryPolicy = null);
}