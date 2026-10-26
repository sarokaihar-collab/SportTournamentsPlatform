using SportsTournament.Application.Common.Outbox;
using SportsTournament.Application.Outbox.Messages;
using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Commands.OpenTournamentRegistration;
using SportsTournament.Application.Tournaments.Interfaces;
using SportsTournament.Domain.Enums;
using SportsTournament.Domain.Exceptions;
using SportsTournament.Domain.Tournaments;
using SportsTournament.UnitTests.Common.Fakes;

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
        var cacheService = new FakeCacheService();
        var outboxWriter = new FakeOutboxMessageWriter();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new OpenTournamentRegistrationHandler(repository, cacheService, outboxWriter, unitOfWork);

        var result = await handler.HandleAsync(new OpenTournamentRegistrationCommand(tournament.Id), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(TournamentStatus.RegistrationOpen, tournament.Status);

        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        Assert.Equal(2, outboxWriter.Messages.Count);

        var listMessage = Assert.Single(outboxWriter.Messages, message => message.MessageType == OutboxMessageTypes.TournamentListCacheInvalidationRequested);

        Assert.IsType<TournamentListCacheInvalidationRequested>(listMessage.Payload);

        var detailsMessage = Assert.Single(outboxWriter.Messages, message => message.MessageType == OutboxMessageTypes.TournamentDetailsCacheInvalidationRequested);

        var detailsPayload = Assert.IsType<TournamentDetailsCacheInvalidationRequested>(detailsMessage.Payload);

        Assert.Equal(tournament.Id, detailsPayload.TournamentId);

        Assert.Equal(2, cacheService.RemovedKeys.Count);
        Assert.Contains(TournamentCacheKeys.List, cacheService.RemovedKeys);
        Assert.Contains(TournamentCacheKeys.Details(tournament.Id), cacheService.RemovedKeys);
    }

    [Fact]
    public async Task HandleAsync_WhenTournamentDoesNotExist_ReturnsFalse()
    {
        var repository = new FakeTournamentRepository();
        var cacheService = new FakeCacheService();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new OpenTournamentRegistrationHandler(repository, cacheService, new FakeOutboxMessageWriter(), unitOfWork);

        var result = await handler.HandleAsync(new OpenTournamentRegistrationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Empty(cacheService.RemovedKeys);
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

        var cacheService = new FakeCacheService();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new OpenTournamentRegistrationHandler(repository, cacheService, new FakeOutboxMessageWriter(), unitOfWork);
        
        await Assert.ThrowsAsync<DomainOperationNotAllowedException>(
            () => handler.HandleAsync(new OpenTournamentRegistrationCommand(tournament.Id), CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Empty(cacheService.RemovedKeys);
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

        public Task<IReadOnlyList<Tournament>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<Tournament> tournaments = Tournament is null ? Array.Empty<Tournament>() : new[] { Tournament };

            return Task.FromResult(tournaments);
        }

        public Task<Tournament?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Tournament?.Id == id ? Tournament : null);
        }

        public Task<Tournament?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Tournament?.Id == id ? Tournament : null);
        }

        public void Add(Tournament tournament)
        {
            Tournament = tournament;
        }
    }
}