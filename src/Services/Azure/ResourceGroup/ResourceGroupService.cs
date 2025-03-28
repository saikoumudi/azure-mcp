using AzureMCP.Arguments;
using AzureMCP.Models.ResourceGroup;
using AzureMCP.Services.Interfaces;

namespace AzureMCP.Services.Azure.ResourceGroup;

public class ResourceGroupService(ICacheService cacheService, ISubscriptionService subscriptionService) : BaseAzureService, IResourceGroupService
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private const string CACHE_KEY = "resourcegroups";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1);

    public async Task<List<ResourceGroupInfo>> GetResourceGroups(string subscriptionId, string? tenantId = null, RetryPolicyArguments? retryPolicy = null)
    {
        ValidateRequiredParameters(subscriptionId);

        // Try to get from cache first
        var cacheKey = $"{CACHE_KEY}_{subscriptionId}_{tenantId ?? "default"}";
        var cachedResults = await _cacheService.GetAsync<List<ResourceGroupInfo>>(cacheKey, CACHE_DURATION);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        // If not in cache, fetch from Azure
        try
        {
            var subscription = await _subscriptionService.GetSubscription(subscriptionId, tenantId, retryPolicy);
            var resourceGroups = await subscription.GetResourceGroups()
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