using Microsoft.Extensions.Caching.Memory;
using ServiceName.Application.Interfaces;

namespace ServiceName.IntegrationTests.Helpers;

// Replaces the Redis-based cache to avoid external dependencies,
// making tests faster, deterministic, and runnable without Docker Redis.
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheService()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration)
    {
        _cache.Set(key, value, expiration);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}