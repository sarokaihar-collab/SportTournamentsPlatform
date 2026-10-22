using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using SportsTournament.Infrastructure.Caching;
using StackExchange.Redis;

namespace SportsTournament.IntegrationTests.Infrastructure;

public sealed class RedisCacheServiceRetryTests
{
    [Fact]
    public async Task RemoveAsync_WhenFirstAttemptFails_RetriesAndSucceeds()
    {
        // Arrange
        var distributedCache = new FakeDistributedCache((attempt, _) => attempt == 1
                ? Task.FromException(new RedisException("Simulated temporary failure."))
                : Task.CompletedTask);

        var service = CreateService(distributedCache);

        // Act
        await service.RemoveAsync("test-key");

        // Assert
        Assert.Equal(2, distributedCache.RemoveCallCount);
    }

    [Fact]
    public async Task RemoveAsync_WhenAllAttemptsFail_StopsAfterMaximumAttempts()
    {
        // Arrange
        var distributedCache = new FakeDistributedCache((_, _) => Task.FromException(new RedisException("Simulated persistent failure.")));

        var service = CreateService(distributedCache);

        // Act
        await service.RemoveAsync("test-key");

        // Assert
        Assert.Equal(3, distributedCache.RemoveCallCount);
    }

    [Fact]
    public async Task RemoveAsync_WhenCancellationOccursDuringRetry_StopsRetrying()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        var distributedCache = new FakeDistributedCache(
            (attempt, _) =>
            {
                if (attempt == 1)
                {
                    cancellationTokenSource.Cancel();

                    return Task.FromException(new RedisException("Simulated temporary failure."));
                }

                return Task.CompletedTask;
            });

        var service = CreateService(distributedCache);

        // Act
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RemoveAsync("test-key", cancellationTokenSource.Token));

        // Assert
        Assert.Equal(1, distributedCache.RemoveCallCount);
    }

    private static RedisCacheService CreateService(IDistributedCache distributedCache)
    {
        return new RedisCacheService(distributedCache, NullLogger<RedisCacheService>.Instance);
    }

    private sealed class FakeDistributedCache(Func<int, CancellationToken, Task> removeAsync) : IDistributedCache
    {
        public int RemoveCallCount { get; private set; }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            RemoveCallCount++;

            return removeAsync(RemoveCallCount, token);
        }

        public byte[]? Get(string key) => throw new NotSupportedException();

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => throw new NotSupportedException();

        public void Refresh(string key) => throw new NotSupportedException();

        public Task RefreshAsync(string key, CancellationToken token = default) => throw new NotSupportedException();

        public void Remove(string key) => throw new NotSupportedException();

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw new NotSupportedException();

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) =>
            throw new NotSupportedException();
    }
}