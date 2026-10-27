using SportTournaments.Application.Common.Interfaces;
using SportTournaments.Application.Common.Outbox;
using SportTournaments.Application.Common.Outbox.Interfaces;
using SportTournaments.Application.Outbox.Messages;
using SportTournaments.Application.Tournaments;
using SportTournaments.Application.Tournaments.Commands.CreateTournament;
using SportTournaments.Application.Tournaments.Interfaces;
using SportTournaments.Contracts.Events.Tournaments;
using SportTournaments.Domain.Tournaments;

public sealed class CreateTournamentHandler(ITournamentRepository tournamentRepository, ICacheService cacheService,
    IOutboxMessageWriter outboxMessageWriter, IUnitOfWork unitOfWork) : ICreateTournamentHandler
{
    public async Task<Guid> HandleAsync(CreateTournamentCommand command, CancellationToken cancellationToken)
    {
        var tournament = new Tournament(
            command.Name,
            command.OrganizerId,
            command.RegistrationDeadline,
            command.StartDate,
            command.EndDate,
            command.MaxTeams,
            command.PlayersOnFieldPerTeam);

        tournamentRepository.Add(tournament);

        outboxMessageWriter.Enqueue(OutboxMessageTypes.TournamentListCacheInvalidationRequested,
            new TournamentListCacheInvalidationRequested());

        var tournamentCreated = new TournamentCreatedV1(
           EventId: Guid.NewGuid(),
           TournamentId: tournament.Id,
           OrganizerId: tournament.OrganizerId,
           TournamentName: tournament.Name,
           OccurredAtUtc: DateTimeOffset.UtcNow);

        outboxMessageWriter.Enqueue(OutboxMessageTypes.TournamentCreated, tournamentCreated);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveAsync(TournamentCacheKeys.List, cancellationToken);

        return tournament.Id;
    }
}