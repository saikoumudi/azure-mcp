using AzureMCP.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureMCP.Services.Caching;

public class CacheService(IMemoryCache memoryCache) : ICacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public Task<T?> GetAsync<T>(string key, TimeSpan? expiration = null)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out T? value) ? value : default);
    }

    public Task SetAsync<T>(string key, T data, TimeSpan? expiration = null)
    {
        if (data == null) return Task.CompletedTask;

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        _memoryCache.Set(key, data, options);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }
}