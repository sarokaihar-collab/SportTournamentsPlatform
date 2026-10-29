using SportTournaments.Application.Common.Outbox;
using SportTournaments.Application.Repositories;
using SportTournaments.Application.Tournaments;
using SportTournaments.Application.Tournaments.Commands.CreateTournament;
using SportTournaments.Contracts.Events.Tournaments;
using SportTournaments.Domain.Common;
using SportTournaments.Domain.Tournaments;
using SportTournaments.UnitTests.Common.Fakes;

namespace SportTournaments.UnitTests.Tournaments.Commands.CreateTournament;

public sealed class CreateTournamentHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCreationSucceeds_CreatesTournamentAndInvalidatesListCache()
    {
        // Arrange
        var command = CreateCommand();
        var repository = new FakeTournamentRepository();
        var userRepository = new FakeUserRepository();
        var cacheService = new FakeCacheService();
        var outboxWriter = new FakeOutboxMessageWriter();

        var handler = new CreateTournamentHandler(repository, cacheService, outboxWriter, new FakeUnitOfWork(), userRepository);

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

        // Assert: the integration event was added to the Outbox.
        Assert.Equal(2, outboxWriter.Messages.Count);

        var createdMessage = Assert.Single(outboxWriter.Messages, message => message.MessageType == OutboxMessageTypes.TournamentCreated);

        var payload = Assert.IsType<TournamentCreatedV1>(createdMessage.Payload);

        Assert.NotEqual(Guid.Empty, payload.EventId);
        Assert.Equal(createdTournament.Id, payload.TournamentId);
        Assert.Equal(command.OrganizerId, payload.OrganizerId);
        Assert.Equal(command.Name, payload.TournamentName);
        Assert.NotEqual(default(DateTimeOffset), payload.OccurredAtUtc);
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

        var userRepository = new FakeUserRepository();

        var cacheService = new FakeCacheService();

        var handler = new CreateTournamentHandler(repository, cacheService, new FakeOutboxMessageWriter(), new FakeUnitOfWork(), userRepository);

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
            new DateTimeOffset(2026, 11, 6, 18, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 11, 7),
            new DateOnly(2026, 11, 8),
            8,
            11);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private User? _user = new User("Test User", "Test@gmail.com");

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = _user;

            return Task.FromResult<User?>(user);
        }
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