// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Services.Azure.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace AzureMcp.Tests.Services.Azure.Authentication;

public class TokenCredentialManagerTests
{
    private readonly TokenCredentialManager _manager;
    private readonly IMemoryCache _mockCache;

    public TokenCredentialManagerTests()
    {
        _mockCache = new MemoryCache(new MemoryCacheOptions());
        _manager = new TokenCredentialManager(_mockCache);
    }

    [Fact]
    public void GetSharedCredential_ReturnsNonNullCredential()
    {
        var credential = _manager.GetSharedCredential();
        Assert.NotNull(credential);
        Assert.IsType<CachedTokenCredential>(credential);
    }

    [Fact]
    public void GetOrCreateTenantCredential_CreatesAndCachesCredential()
    {
        var tenantId = "sample-tenant-id";
        var credential1 = _manager.GetOrCreateTenantCredential(tenantId);
        var credential2 = _manager.GetOrCreateTenantCredential(tenantId);

        Assert.NotNull(credential1);
        Assert.Same(credential1, credential2); // should return cached
    }
}
