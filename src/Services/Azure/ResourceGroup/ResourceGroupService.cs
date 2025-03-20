using Azure.ResourceManager;
using Azure.Core.Extensions;
using AzureMCP.Models;
using AzureMCP.Services.Interfaces;
using AzureMCP.Arguments;
using AzureMCP.Models.ResourceGroup;
using Azure.Core;

namespace AzureMCP.Services.Azure.ResourceGroup;

public class ResourceGroupService : BaseAzureService, IResourceGroupService
{
    private readonly ICacheService _cacheService;
    private const string CACHE_KEY = "resourcegroups";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1);

    public ResourceGroupService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<List<ResourceGroupInfo>> GetResourceGroups(string subscriptionId, string? tenantId = null, RetryPolicyArguments? retryPolicy = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(subscriptionId);

        // Try to get from cache first
        var cacheKey = $"{CACHE_KEY}_{subscriptionId}_{tenantId ?? "default"}";
        var cachedResults = await _cacheService.GetAsync<List<ResourceGroupInfo>>(cacheKey, CACHE_DURATION);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        // If not in cache, fetch from Azure
        var armClient = CreateArmClient(tenantId, retryPolicy);
        try
        {
            var subscription = await armClient.GetSubscriptionResource(
                new ResourceIdentifier($"/subscriptions/{subscriptionId}"))
                .GetAsync();

            var resourceGroups = await subscription.Value.GetResourceGroups()
                .GetAllAsync()
                .Select(rg => new ResourceGroupInfo(
                    rg.Data.Name,
                    rg.Data.Id.ToString(),
                    rg.Data.Location.ToString()))
                .ToListAsync();

            // Cache the results
            await _cacheService.SetAsync(cacheKey, resourceGroups, CACHE_DURATION);
            
            return resourceGroups;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving resource groups: {ex.Message}", ex);
        }
    }
}