// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Extensions.Caching.Memory;

namespace AzureMcp.Services.Azure.Authentication;

public class TokenCredentialManager
{
    private readonly CachedTokenCredential _sharedCredential;
    private readonly IMemoryCache _tenantCache;
    private static readonly string[] ScopesToWarm = new[]
    {
        "https://management.azure.com/.default",
        "https://cosmos.azure.com/.default",
        "https://monitor.azure.com/.default",
        "https://storage.azure.com/.default",
        "https://azconfig.io/.default"
    };

    public TokenCredentialManager(IMemoryCache? memoryCache = null)
    {
        _tenantCache = memoryCache ?? new MemoryCache(new MemoryCacheOptions());
        _sharedCredential = new CachedTokenCredential(new CustomChainedCredential());
    }

    public TokenCredential GetSharedCredential() => _sharedCredential;

    public TokenCredential GetOrCreateTenantCredential(string tenantId)
    {
        if (_tenantCache.TryGetValue(tenantId, out var obj) && obj is TokenCredential cached)
        {
            return cached;
        }

        var credential = new CachedTokenCredential(new CustomChainedCredential(tenantId));
        _tenantCache.Set(tenantId, credential, TimeSpan.FromHours(12));
        return credential;
    }

    public async Task WarmupSharedTokenAsync()
    {
        var scopeTasks = ScopesToWarm.Select(async scope =>
        {
            try
            {
                var context = new TokenRequestContext(new[] { scope });
                _ = await _sharedCredential.GetTokenAsync(context, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Shared token warmup failed for scope {scope}: {ex.Message}");
            }
        });

        await Task.WhenAll(scopeTasks);
    }

    public async Task WarmupTenantTokensAsync(IEnumerable<string> tenantIds)
    {
        var warmupTasks = tenantIds.Select(async tenantId =>
        {
            var credential = GetOrCreateTenantCredential(tenantId);

            var scopeTasks = ScopesToWarm.Select(async scope =>
            {
                try
                {
                    var context = new TokenRequestContext(new[] { scope });
                    _ = await credential.GetTokenAsync(context, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Tenant {tenantId} warmup failed for scope {scope}: {ex.Message}");
                }
            });

            await Task.WhenAll(scopeTasks);
        });

        await Task.WhenAll(warmupTasks);
    }
}
