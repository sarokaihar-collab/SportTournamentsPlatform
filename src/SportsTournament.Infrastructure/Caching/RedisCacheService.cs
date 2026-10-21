using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SportsTournament.Application.Common;
using StackExchange.Redis;

namespace SportsTournament.Infrastructure.Caching;

public sealed class RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        string? json;

        try
        {
            json = await distributedCache.GetStringAsync(key, cancellationToken);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Redis read failed for cache key {CacheKey}. Treating as cache miss.", key);
            return null;
        }

        if (json is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Failed to deserialize cached value for key {CacheKey}. Treating as cache miss.", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var json = JsonSerializer.Serialize(value, JsonOptions);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        try
        {
            await distributedCache.SetStringAsync(key, json, options, cancellationToken);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Redis write failed for cache key {CacheKey}.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Redis removal failed for cache key {CacheKey}.", key);
        }
    }
}