using Microsoft.Extensions.Logging;
using ServiceName.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace ServiceName.Infrastructure.Caching;

/// Cache is treated as OPTIONAL infrastructure.
/// Failures (connection issues, timeouts) must NOT break core business operations.
/// 
/// Behavior:
/// - Cache hit → return data
/// - Cache miss → return null (fallback to DB in Application layer)
/// - Cache unavailable → behave like cache miss
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);

            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex,
                "Redis unavailable. Cache GET failed for key {CacheKey}", key);

            return default; // behave like cache miss
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex,
                "Redis timeout. Cache GET failed for key {CacheKey}", key);

            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);

            await _db.StringSetAsync(key, json, ttl);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex,
                "Redis unavailable. Cache SET failed for key {CacheKey}", key);
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex,
                "Redis timeout. Cache SET failed for key {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex,
                "Redis unavailable. Cache REMOVE failed for key {CacheKey}", key);
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex,
                "Redis timeout. Cache REMOVE failed for key {CacheKey}", key);
        }
    }
}