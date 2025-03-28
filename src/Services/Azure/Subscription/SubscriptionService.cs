using Azure.Core;
using Azure.ResourceManager.Resources;
using AzureMCP.Arguments;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;

namespace AzureMCP.Services.Azure.Subscription;

public class SubscriptionService(ICacheService cacheService) : BaseAzureService, ISubscriptionService
{
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private const string CACHE_KEY = "subscriptions";
    private const string SUBSCRIPTION_CACHE_KEY = "subscription";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(12);

    public async Task<List<ArgumentOption>> GetSubscriptions(string? tenantId = null, RetryPolicyArguments? retryPolicy = null)
    {
        // Try to get from cache first
        var cacheKey = tenantId == null ? CACHE_KEY : $"{CACHE_KEY}_{tenantId}";
        var cachedResults = await _cacheService.GetAsync<List<ArgumentOption>>(cacheKey, CACHE_DURATION);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        // If not in cache, fetch from Azure
        var armClient = CreateArmClient(tenantId, retryPolicy);
        var subscriptions = armClient.GetSubscriptions();
        var results = new List<ArgumentOption>();

        foreach (var subscription in subscriptions)
        {
            results.Add(new ArgumentOption
            {
                Name = subscription.Data.DisplayName,
                Id = subscription.Data.SubscriptionId
            });
        }

        // Cache the results
        await _cacheService.SetAsync(cacheKey, results, CACHE_DURATION);

        return results;
    }

    public async Task<SubscriptionResource> GetSubscription(string subscriptionId, string? tenantId = null, RetryPolicyArguments? retryPolicy = null)
    {
        ValidateRequiredParameters(subscriptionId);

        // Try to get from cache first
        var cacheKey = tenantId == null ? $"{SUBSCRIPTION_CACHE_KEY}_{subscriptionId}" : $"{SUBSCRIPTION_CACHE_KEY}_{subscriptionId}_{tenantId}";
        var cachedSubscription = await _cacheService.GetAsync<SubscriptionResource>(cacheKey, CACHE_DURATION);
        if (cachedSubscription != null)
        {
            return cachedSubscription;
        }

        var armClient = CreateArmClient(tenantId, retryPolicy);
        var response = await armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId)).GetAsync();

        if (response?.Value == null)
        {
            throw new Exception($"Could not retrieve subscription {subscriptionId}");
        }

        // Cache the result
        await _cacheService.SetAsync(cacheKey, response.Value, CACHE_DURATION);

        return response.Value;
    }
}