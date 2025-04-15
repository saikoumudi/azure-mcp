// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Extensions.Caching.Memory;

namespace AzureMcp.Services.Azure.Authentication;

public class CachingTokenCredential : TokenCredential
{
    private readonly TokenCredential _innerCredential;

    private readonly MemoryCache _cache = new(new MemoryCacheOptions
    {
        SizeLimit = 100 // Max number of unique tokens to cache
    });

    public CachingTokenCredential(TokenCredential innerCredential)
    {
        _innerCredential = innerCredential ?? throw new ArgumentNullException(nameof(innerCredential));
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var key = GetCacheKey(requestContext);

        if (_cache.TryGetValue(key, out AccessToken token) && token.ExpiresOn > DateTimeOffset.UtcNow)
        {
            return token;
        }

        var newToken = _innerCredential.GetToken(requestContext, cancellationToken);
        CacheToken(key, newToken);
        return newToken;
    }

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var key = GetCacheKey(requestContext);

        if (_cache.TryGetValue(key, out AccessToken token) && token.ExpiresOn > DateTimeOffset.UtcNow)
        {
            return token;
        }

        var newToken = await _innerCredential.GetTokenAsync(requestContext, cancellationToken).ConfigureAwait(false);
        CacheToken(key, newToken);
        return newToken;
    }

    private void CacheToken(string key, AccessToken token)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = token.ExpiresOn,
            Size = 1
        };

        _cache.Set(key, token, options);
    }

    private static string GetCacheKey(TokenRequestContext context)
    {
        var scopesKey = context.Scopes != null ? string.Join(",", context.Scopes) : string.Empty;
        var claimsKey = context.Claims ?? string.Empty;
        var tenantKey = context.TenantId ?? string.Empty;
        var caeKey = context.IsCaeEnabled ? "cae=true" : "cae=false";

        return $"{scopesKey}|{claimsKey}|{tenantKey}|{caeKey}";
    }
}
