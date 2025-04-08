using Azure.Identity;
using Azure.ResourceManager;
using AzureMCP.Models.Argument;
using AzureMCP.Services.Interfaces;

namespace AzureMCP.Services.Azure.Tenant;

public class TenantService(ICacheService cacheService) : BaseAzureService, ITenantService
{
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private const string CACHE_KEY = "tenants";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(12);

    public async Task<List<ArgumentOption>> GetTenants()
    {
        // Try to get from cache first
        var cachedResults = await _cacheService.GetAsync<List<ArgumentOption>>(CACHE_KEY, CACHE_DURATION);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        // If not in cache, fetch from Azure
        var results = new List<ArgumentOption>();
        var credential = new DefaultAzureCredential();
        var options = AddDefaultPolicies(new ArmClientOptions());
        var client = new ArmClient(credential, default, options);

        await foreach (var tenant in client.GetTenants())
        {
            var tenantId = tenant.Data.TenantId?.ToString() ?? throw new InvalidOperationException("Tenant ID cannot be null");
            results.Add(new ArgumentOption
            {
                Name = tenant.Data.DisplayName,
                Id = tenantId
            });
        }

        // Cache the results
        await _cacheService.SetAsync(CACHE_KEY, results, CACHE_DURATION);
        return results;
    }

    public bool IsTenantId(string tenant)
    {
        return Guid.TryParse(tenant, out _);
    }

    public async Task<string> GetTenantId(string tenant)
    {
        if (IsTenantId(tenant))
        {
            return tenant;
        }

        return await GetTenantIdByName(tenant);
    }

    public async Task<string> GetTenantIdByName(string tenantName)
    {
        var tenants = await GetTenants();
        var tenant = tenants.FirstOrDefault(t => t.Name.Equals(tenantName, StringComparison.OrdinalIgnoreCase));

        if (tenant == null)
        {
            throw new Exception($"Could not find tenant with name {tenantName}");
        }

        return tenant.Id;
    }

    public async Task<string> GetTenantNameById(string tenantId)
    {
        var tenants = await GetTenants();
        var tenant = tenants.FirstOrDefault(t => t.Id.Equals(tenantId, StringComparison.OrdinalIgnoreCase));

        if (tenant == null)
        {
            throw new Exception($"Could not find tenant with ID {tenantId}");
        }

        return tenant.Name;
    }
}