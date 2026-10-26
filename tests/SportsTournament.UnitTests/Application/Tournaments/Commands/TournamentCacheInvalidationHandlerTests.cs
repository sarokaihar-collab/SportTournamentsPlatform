using SportsTournament.Application.Outbox.Messages;
using SportsTournament.Application.Tournaments;
using SportsTournament.UnitTests.Common.Fakes;

namespace SportsTournament.UnitTests.Tournaments;

public sealed class TournamentCacheInvalidationHandlerTests
{
    private readonly FakeCacheService _cacheService = new();

    private TournamentCacheInvalidationHandler CreateSut() => new(_cacheService);

    [Fact]
    public async Task HandleAsync_WhenListInvalidationRequested_RemovesListKey()
    {
        // Arrange
        var message = new TournamentListCacheInvalidationRequested();
        var sut = CreateSut();

        // Act
        await sut.HandleAsync(message);

        // Assert
        Assert.Equal(new[] { TournamentCacheKeys.List }, _cacheService.RemovedKeys);
    }

    [Fact]
    public async Task HandleAsync_WhenDetailsInvalidationRequested_RemovesDetailsKey()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var message = new TournamentDetailsCacheInvalidationRequested(tournamentId);

        var sut = CreateSut();

        // Act
        await sut.HandleAsync(message);

        // Assert
        Assert.Equal(new[] { TournamentCacheKeys.Details(tournamentId) }, _cacheService.RemovedKeys);
    }

    [Fact]
    public async Task HandleAsync_WhenListRemovalFails_ThrowsException()
    {
        // Arrange
        _cacheService.FailedRemovalKeys.Add(TournamentCacheKeys.List);

        var message = new TournamentListCacheInvalidationRequested();
        var sut = CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(message));

        // Assert
        Assert.Contains(TournamentCacheKeys.List, exception.Message);
        Assert.Equal(new[] { TournamentCacheKeys.List }, _cacheService.RemovedKeys);
    }

    [Fact]
    public async Task HandleAsync_WhenDetailsRemovalFails_ThrowsException()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var detailsKey = TournamentCacheKeys.Details(tournamentId);

        _cacheService.FailedRemovalKeys.Add(detailsKey);

        var message = new TournamentDetailsCacheInvalidationRequested(tournamentId);

        var sut = CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(message));

        // Assert
        Assert.Contains(detailsKey, exception.Message);
        Assert.Equal(new[] { detailsKey }, _cacheService.RemovedKeys);
    }

    [Fact]
    public async Task HandleAsync_WhenTournamentIdIsEmpty_ThrowsWithoutRemovingKey()
    {
        // Arrange
        var message = new TournamentDetailsCacheInvalidationRequested(Guid.Empty);

        var sut = CreateSut();

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(message));

        // Assert
        Assert.Empty(_cacheService.RemovedKeys);
    }
}