using Microsoft.Extensions.Options;
using SportsTournament.Application.Common;
using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Queries;
using SportsTournament.Domain.Tournaments;
using SportsTournament.UnitTests.Common.Fakes;

namespace SportsTournament.UnitTests.Tournaments.Queries.TournamentDetails;

public sealed class TournamentDetailsQueryTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCacheContainsDetails_ReturnsCachedDetailsWithoutRepositoryCall()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var repository = new FakeTournamentRepository();
        var cacheService = new FakeCacheService();

        var cachedItem = new TournamentListItem(
            tournamentId,
            "Cached Tournament",
            new DateOnly(2026, 11, 7),
            new DateOnly(2026, 11, 8),
            8,
            11,
            "RegistrationOpen");

        await cacheService.SetAsync(TournamentCacheKeys.Details(tournamentId), cachedItem, TimeSpan.FromMinutes(5));

        var query = new TournamentDetailsQuery(
            repository,
            cacheService,
            CreateSettings());

        // Act
        var result = await query.ExecuteAsync(tournamentId, CancellationToken.None);

        // Assert
        Assert.Equal(cachedItem, result);
        Assert.Equal(0, repository.GetByIdCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCacheIsEmptyAndTournamentExists_LoadsAndCachesDetails()
    {
        // Arrange
        var tournament = CreateTournament();

        var repository = new FakeTournamentRepository();
        repository.Tournaments.Add(tournament);

        var cacheService = new FakeCacheService();

        var settings = Options.Create(new TournamentCacheSettings
        {
            ListTtl = TimeSpan.FromMinutes(5),
            DetailsTtl = TimeSpan.FromMinutes(3)
        });

        var query = new TournamentDetailsQuery(repository, cacheService, settings);

        var cacheKey = TournamentCacheKeys.Details(tournament.Id);

        // Act
        var result = await query.ExecuteAsync(tournament.Id, CancellationToken.None);

        // Assert: the repository was called once.
        Assert.Equal(1, repository.GetByIdCallCount);

        // Assert: the entity was mapped to a DTO.
        var expected = ToListItem(tournament);

        Assert.Equal(expected, result);

        // Assert: the result was saved using the details key.
        var cachedResult =
            await cacheService.GetAsync<TournamentListItem>(cacheKey);

        Assert.Equal(expected, cachedResult);

        // Assert: the configured details TTL was used.
        Assert.Equal(settings.Value.DetailsTtl, cacheService.SetTtls[cacheKey]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTournamentDoesNotExist_ReturnsNullWithoutCaching()
    {
        // Arrange
        var repository = new FakeTournamentRepository();
        var cacheService = new FakeCacheService();
        var query = new TournamentDetailsQuery(repository, cacheService, CreateSettings());

        var tournamentId = Guid.NewGuid();

        // Act
        var result = await query.ExecuteAsync(tournamentId, CancellationToken.None);

        // Assert
        Assert.Null(result);
        Assert.Equal(1, repository.GetByIdCallCount);
        Assert.Empty(cacheService.SetTtls);
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