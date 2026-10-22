using Microsoft.Extensions.Options;
using SportsTournament.Application.Common;
using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Queries;
using SportsTournament.Domain.Tournaments;
using SportsTournament.UnitTests.Common.Fakes;

namespace SportsTournament.UnitTests.Tournaments.Queries.TournamentList;

public sealed class TournamentListQueryTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCacheContainsList_ReturnsCachedListWithoutRepositoryCall()
    {
        // Arrange
        var repository = new FakeTournamentRepository();
        var cacheService = new FakeCacheService();

        var cachedList = new List<TournamentListItem>
        {
            new(
                Guid.NewGuid(),
                "Cached Tournament",
                new DateOnly(2026, 11, 7),
                new DateOnly(2026, 11, 8),
                8,
                11,
                "RegistrationOpen")
        };

        await cacheService.SetAsync(TournamentCacheKeys.List, cachedList, TimeSpan.FromMinutes(5));

        var settings = CreateSettings();

        var query = new TournamentListQuery(repository, cacheService, settings);

        // Act
        var result = await query.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.Equal(cachedList, result);
        Assert.Equal(0, repository.GetAllCallCount);
        Assert.Empty(repository.Tournaments);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCacheIsEmpty_LoadsFromRepositoryAndCachesResult()
    {
        // Arrange
        var tournament = CreateTournament();

        var repository = new FakeTournamentRepository();
        repository.Tournaments.Add(tournament);

        var cacheService = new FakeCacheService();

        var settings = Options.Create(new TournamentCacheSettings
        {
            ListTtl = TimeSpan.FromMinutes(7),
            DetailsTtl = TimeSpan.FromMinutes(3)
        });

        var query = new TournamentListQuery(repository, cacheService, settings);

        // Act
        var result = await query.ExecuteAsync(CancellationToken.None);

        // Assert: the repository was called once.
        Assert.Equal(1, repository.GetAllCallCount);

        // Assert: the domain entity was mapped to a DTO.
        Assert.Equal(new[] { ToListItem(tournament) }, result.ToArray());

        // Assert: the result was saved to the cache.
        var cachedResult = await cacheService.GetAsync<List<TournamentListItem>>(TournamentCacheKeys.List);

        Assert.NotNull(cachedResult);
        Assert.Equal(result.ToArray(), cachedResult);

        // Assert: the configured list TTL was used.
        Assert.Equal(settings.Value.ListTtl, cacheService.SetTtls[TournamentCacheKeys.List]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryReturnsNoTournaments_CachesEmptyList()
    {
        // Arrange
        var repository = new FakeTournamentRepository();
        var cacheService = new FakeCacheService();
        var settings = CreateSettings();

        var query = new TournamentListQuery(repository, cacheService, settings);

        // Act
        var result = await query.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.Empty(result);
        Assert.Equal(1, repository.GetAllCallCount);

        var cachedResult = await cacheService.GetAsync<List<TournamentListItem>>(TournamentCacheKeys.List);

        Assert.NotNull(cachedResult);
        Assert.Empty(cachedResult);

        Assert.Equal(settings.Value.ListTtl, cacheService.SetTtls[TournamentCacheKeys.List]);
    }

    private static IOptions<TournamentCacheSettings> CreateSettings()
    {
        return Options.Create(new TournamentCacheSettings
        {
            ListTtl = TimeSpan.FromMinutes(5),
            DetailsTtl = TimeSpan.FromMinutes(3)
        });
    }

    private static Tournament CreateTournament()
    {
        return new Tournament(
            "Test Tournament",
            Guid.NewGuid(),
            new DateTimeOffset(
                2026, 11, 6, 18, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 11, 7),
            new DateOnly(2026, 11, 8),
            8,
            11);
    }

    private static TournamentListItem ToListItem(Tournament tournament)
    {
        return new TournamentListItem(
            tournament.Id,
            tournament.Name,
            tournament.StartDate,
            tournament.EndDate,
            tournament.MaxTeams,
            tournament.PlayersOnFieldPerTeam,
            tournament.Status.ToString());
    }
}