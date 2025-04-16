// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using AzureMcp.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace AzureMcp.Tests.Services.Azure.Authentication;

public class CachingTokenCredentialTests
{
    private static readonly string[] Scopes = new[] { "https://example.com/.default" };
    private static readonly TokenRequestContext RequestContext = new(Scopes);

    [Fact]
    public async Task ReturnsTokenFromInnerCredential_IfNotCached()
    {
        var expected = new AccessToken("token123", DateTimeOffset.UtcNow.AddMinutes(5));
        var inner = Substitute.For<TokenCredential>();
        inner.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
             .Returns(expected);

        var caching = new CachedTokenCredential(inner);

        var result = await caching.GetTokenAsync(RequestContext, CancellationToken.None);

        Assert.Equal(expected.Token, result.Token);
        await inner.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReturnsCachedToken_IfNotExpired()
    {
        var expected = new AccessToken("cached-token", DateTimeOffset.UtcNow.AddMinutes(5));
        var inner = Substitute.For<TokenCredential>();
        inner.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
             .Returns(expected);

        var caching = new CachedTokenCredential(inner);

        var first = await caching.GetTokenAsync(RequestContext, CancellationToken.None);
        var second = await caching.GetTokenAsync(RequestContext, CancellationToken.None);

        Assert.Equal(first.Token, second.Token);
        await inner.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshesToken_IfExpired()
    {
        var expired = new AccessToken("expired", DateTimeOffset.UtcNow.AddMinutes(-2));
        var fresh = new AccessToken("fresh", DateTimeOffset.UtcNow.AddMinutes(5));

        var inner = Substitute.For<TokenCredential>();
        inner.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
             .Returns(_ => expired, _ => fresh);

        var caching = new CachedTokenCredential(inner);

        var first = await caching.GetTokenAsync(RequestContext, CancellationToken.None);
        var second = await caching.GetTokenAsync(RequestContext, CancellationToken.None);

        Assert.Equal("fresh", second.Token);
        await inner.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CachesToken_PerRequestContext()
    {
        var tokenA = new AccessToken("token-a", DateTimeOffset.UtcNow.AddMinutes(5));
        var tokenB = new AccessToken("token-b", DateTimeOffset.UtcNow.AddMinutes(5));

        var inner = Substitute.For<TokenCredential>();
        inner.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
             .Returns(tokenA, tokenB);

        var caching = new CachedTokenCredential(inner);

        var context1 = new TokenRequestContext(new[] { "https://a.com/.default" });
        var context2 = new TokenRequestContext(new[] { "https://b.com/.default" });

        var result1 = await caching.GetTokenAsync(context1, CancellationToken.None);
        var result2 = await caching.GetTokenAsync(context2, CancellationToken.None);

        Assert.Equal("token-a", result1.Token);
        Assert.Equal("token-b", result2.Token);
        await inner.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandlesConcurrentAccess_WithoutErrors()
    {
        var token = new AccessToken("concurrent-token", DateTimeOffset.UtcNow.AddMinutes(5));

        var inner = Substitute.For<TokenCredential>();
        inner.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
             .Returns(token);

        var caching = new CachedTokenCredential(inner);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => caching.GetTokenAsync(RequestContext, CancellationToken.None).AsTask())
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, t => Assert.Equal("concurrent-token", t.Token));

        Assert.All(tasks, t => Assert.Equal("concurrent-token", t.Result.Token));
        await inner.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }
}
