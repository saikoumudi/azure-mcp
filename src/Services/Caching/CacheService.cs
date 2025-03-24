using AzureMCP.Services.Interfaces;
using System.Text.Json;

namespace AzureMCP.Services.Caching;

public class CacheService : ICacheService
{
    private readonly string _cacheDirectory;
    private readonly TimeSpan _defaultExpiration;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public CacheService(string? cacheDirectory = null, TimeSpan? defaultExpiration = null)
    {
        _cacheDirectory = cacheDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "azmcp", "cache");
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromHours(24);

        // Ensure cache directory exists
        Directory.CreateDirectory(_cacheDirectory);
    }

    public async Task<T?> GetAsync<T>(string key, TimeSpan? expiration = null)
    {
        var cacheFile = GetCacheFilePath(key);
        if (!File.Exists(cacheFile)) return default;

        try
        {
            var json = await File.ReadAllTextAsync(cacheFile);
            var cache = JsonSerializer.Deserialize<CacheEntry<T>>(json);

            if (cache == null || IsExpired<T>(cache, expiration ?? _defaultExpiration))
            {
                await DeleteAsync(key);
                return default;
            }

            return cache.Data;
        }
        catch
        {
            await DeleteAsync(key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan? expiration = null)
    {
        var cacheFile = GetCacheFilePath(key);
        var cache = new CacheEntry<T>
        {
            Timestamp = DateTime.UtcNow,
            Data = data
        };

        var json = JsonSerializer.Serialize(cache, _jsonOptions);
        await File.WriteAllTextAsync(cacheFile, json);
    }

    public Task DeleteAsync(string key)
    {
        var cacheFile = GetCacheFilePath(key);
        if (File.Exists(cacheFile))
        {
            File.Delete(cacheFile);
        }
        return Task.CompletedTask;
    }

    private string GetCacheFilePath(string key)
    {
        // Create a safe filename from the key
        var safeKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_cacheDirectory, $"{safeKey}.json");
    }

    private static bool IsExpired<T>(CacheEntry<T> cache, TimeSpan expiration)
    {
        return DateTime.UtcNow - cache.Timestamp > expiration;
    }

    private class CacheEntry<T>
    {
        public DateTime Timestamp { get; set; }
        public T? Data { get; set; }
    }
}