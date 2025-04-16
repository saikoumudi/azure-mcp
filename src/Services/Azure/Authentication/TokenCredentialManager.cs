// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Extensions.Caching.Memory;

namespace AzureMcp.Services.Azure.Authentication;

public class TokenCredentialManager
{
    private readonly CachedTokenCredential _sharedCredential;
    private readonly IMemoryCache _tenantCredentialCache;
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
        _tenantCredentialCache = memoryCache ?? new MemoryCache(new MemoryCacheOptions());
        _sharedCredential = new CachedTokenCredential(new CustomChainedCredential());
    }

    public TokenCredential SharedCredential => _sharedCredential;

    public TokenCredential GetOrCreateTenantCredential(string tenantId)
    {
        if (_tenantCredentialCache.TryGetValue(tenantId, out var obj) && obj is TokenCredential cached)
        {
            return cached;
        }

        var credential = new CachedTokenCredential(new CustomChainedCredential(tenantId));
        _tenantCredentialCache.Set(tenantId, credential, TimeSpan.FromHours(12));
        return credential;
    }

    public async Task WarmupSharedTokenAsync()
    {
        await WarmupCredentialAsync(_sharedCredential);
    }

    public async Task WarmupTenantTokensAsync(IEnumerable<string> tenantIds)
    {
        var warmupTasks = tenantIds.Select(tenantId =>
        {
            var credential = GetOrCreateTenantCredential(tenantId);
            return WarmupCredentialAsync(credential);
        });

        await Task.WhenAll(warmupTasks);
    }

    private async Task WarmupCredentialAsync(TokenCredential credential)
    {
        var scopeTasks = ScopesToWarm.Select(async scope =>
        {
            try
            {
                var context = new TokenRequestContext(new[] { scope });
                _ = await credential.GetTokenAsync(context, CancellationToken.None);
            }
            catch (Exception){ /* No-op */}
        });

        await Task.WhenAll(scopeTasks);
    }
}
