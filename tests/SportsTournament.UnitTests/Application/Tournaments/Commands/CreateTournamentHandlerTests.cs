using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Commands.CreateTournament;
using SportsTournament.Domain.Tournaments;
using SportsTournament.UnitTests.Common.Fakes;

namespace SportsTournament.UnitTests.Tournaments.Commands.CreateTournament;

public sealed class CreateTournamentHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCreationSucceeds_CreatesTournamentAndInvalidatesListCache()
    {
        // Arrange
        var command = CreateCommand();
        var repository = new FakeTournamentRepository();
        var cacheService = new FakeCacheService();

        var handler = new CreateTournamentHandler(repository, cacheService, new FakeOutboxMessageWriter(), new FakeUnitOfWork());

        // Act
        var createdTournamentId = await handler.HandleAsync(command, CancellationToken.None);

        // Assert: tournament was created and passed to the repository.
        var createdTournament = Assert.IsType<Tournament>(repository.CreatedTournament);

        Assert.True(repository.CreateCalled);
        Assert.Equal(createdTournament.Id, createdTournamentId);

        Assert.Equal(command.Name, createdTournament.Name);
        Assert.Equal(command.OrganizerId, createdTournament.OrganizerId);
        Assert.Equal(command.RegistrationDeadline, createdTournament.RegistrationDeadlineUtc);
        Assert.Equal(command.StartDate, createdTournament.StartDate);
        Assert.Equal(command.EndDate, createdTournament.EndDate);
        Assert.Equal(command.MaxTeams, createdTournament.MaxTeams);
        Assert.Equal(command.PlayersOnFieldPerTeam, createdTournament.PlayersOnFieldPerTeam);

        // Assert: only the tournament list cache is invalidated.
        Assert.Equal(new[] { TournamentCacheKeys.List }, cacheService.RemovedKeys);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryFails_DoesNotInvalidateCache()
    {
        // Arrange
        var command = CreateCommand();
        var repository = new FakeTournamentRepository
        {
            ThrowOnCreate = true
        };

        var cacheService = new FakeCacheService();

        var handler = new CreateTournamentHandler(repository, cacheService, new FakeOutboxMessageWriter(), new FakeUnitOfWork());

        // Act
        await Assert.ThrowsAsync<RepositoryFailureException>(() => handler.HandleAsync(command, CancellationToken.None));

        // Assert
        Assert.True(repository.CreateCalled);
        Assert.Null(repository.CreatedTournament);
        Assert.Empty(cacheService.RemovedKeys);
    }

    private static CreateTournamentCommand CreateCommand()
    {
        return new CreateTournamentCommand(
            "Test Tournament",
            Guid.NewGuid(),
            new DateTimeOffset(
                2026, 11, 6, 18, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 11, 7),
            new DateOnly(2026, 11, 8),
            8,
            11);
    }

    private sealed class FakeTournamentRepository : ITournamentRepository
    {
        private Tournament? _tournament;

        public Tournament? CreatedTournament => _tournament;

        public bool CreateCalled { get; private set; }

        public bool ThrowOnCreate { get; init; }

        public Task<IReadOnlyList<Tournament>> GetAllAsync(CancellationToken cancellationToken)
        {
            var tournament = _tournament;

            IReadOnlyList<Tournament> tournaments = tournament is null ? Array.Empty<Tournament>() : new[] { tournament };

            return Task.FromResult(tournaments);
        }

        public Task<Tournament?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var tournament = _tournament;

            return Task.FromResult<Tournament?>( tournament?.Id == id ? tournament : null);
        }

        public Task<Tournament?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
        {
            return GetByIdAsync(id, cancellationToken);
        }

        public void Add(Tournament tournament)
        {
            CreateCalled = true;

            if (ThrowOnCreate)
            {
                throw new RepositoryFailureException();
            }

            _tournament = tournament;
        }
    }

    private sealed class RepositoryFailureException : Exception;
}