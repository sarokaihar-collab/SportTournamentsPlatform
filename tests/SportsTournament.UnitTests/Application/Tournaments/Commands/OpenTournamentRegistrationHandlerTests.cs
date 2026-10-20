using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Commands;
using SportsTournament.Application.Tournaments.Commands.OpenTournamentRegistration;
using SportsTournament.Domain.Enums;
using SportsTournament.Domain.Exceptions;
using SportsTournament.Domain.Tournaments;

namespace SportsTournament.UnitTests.Application.Tournaments.Commands;

public sealed class OpenTournamentRegistrationHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenTournamentExists_OpensRegistrationAndSaves()
    {
        var tournament = CreateTournament();
        var repository = new FakeTournamentRepository
        {
            Tournament = tournament
        };
        var handler = new OpenTournamentRegistrationHandler(repository);

        var result = await handler.HandleAsync(
            new OpenTournamentRegistrationCommand(tournament.Id),
            CancellationToken.None);

        Assert.True(result);
        Assert.Equal(TournamentStatus.RegistrationOpen, tournament.Status);
        Assert.True(repository.UpdateCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenTournamentDoesNotExist_ReturnsFalse()
    {
        var repository = new FakeTournamentRepository();
        var handler = new OpenTournamentRegistrationHandler(repository);

        var result = await handler.HandleAsync(
            new OpenTournamentRegistrationCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result);
        Assert.False(repository.UpdateCalled);
    }

    [Fact]
    public async Task HandleAsync_WhenRegistrationIsAlreadyOpen_ThrowsDomainOperationNotAllowedException()
    {
        var tournament = CreateTournament();
        tournament.OpenRegistration(DateTimeOffset.UtcNow);

        var repository = new FakeTournamentRepository
        {
            Tournament = tournament
        };
        var handler = new OpenTournamentRegistrationHandler(repository);

        await Assert.ThrowsAsync<DomainOperationNotAllowedException>(
            () => handler.HandleAsync(
                new OpenTournamentRegistrationCommand(tournament.Id),
                CancellationToken.None));

        Assert.False(repository.UpdateCalled);
    }

    private static Tournament CreateTournament()
    {
        return new Tournament(
            "Test Tournament",
            Guid.NewGuid(),
            new DateTimeOffset(2026, 11, 6, 18, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 11, 7),
            new DateOnly(2026, 11, 8),
            8,
            11); ;
    }

    private sealed class FakeTournamentRepository : ITournamentRepository
    {
        public Tournament? Tournament { get; set; }

        public bool UpdateCalled { get; private set; }

        public Task<IReadOnlyList<Tournament>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            IReadOnlyList<Tournament> tournaments =
                Tournament is null
                    ? Array.Empty<Tournament>()
                    : new[] { Tournament };

            return Task.FromResult(tournaments);
        }

        public Task<Tournament?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Tournament?.Id == id ? Tournament : null);
        }

        public Task<Tournament?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Tournament?.Id == id ? Tournament : null);
        }

        public Task CreateAsync(
            Tournament tournament,
            CancellationToken cancellationToken)
        {
            Tournament = tournament;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            Tournament tournament,
            CancellationToken cancellationToken)
        {
            UpdateCalled = true;
            Tournament = tournament;
            return Task.CompletedTask;
        }
    }
}