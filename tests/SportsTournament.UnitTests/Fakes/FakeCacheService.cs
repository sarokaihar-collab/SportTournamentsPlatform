using SportsTournament.Application.Common;

namespace SportsTournament.UnitTests.Common.Fakes;

public sealed class FakeCacheService : ICacheService
{
    private readonly Dictionary<string, object> _cache = [];

    public List<string> RemovedKeys { get; } = [];

    public Dictionary<string, TimeSpan> SetTtls { get; } = [];

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_cache.TryGetValue(key, out var value) && value is T result)
        {
            return Task.FromResult<T?>(result);
        }

        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(value);

        _cache[key] = value;
        SetTtls[key] = ttl;

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _cache.Remove(key);
        RemovedKeys.Add(key);

        return Task.CompletedTask;
    }
}