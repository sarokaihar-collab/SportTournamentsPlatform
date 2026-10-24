using SportsTournament.Application.Common.Interfaces;
using SportsTournament.Application.Common.Outbox;
using SportsTournament.Application.Common.Outbox.Interfaces;
using SportsTournament.Application.Outbox.Messages;
using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Commands.CreateTournament;
using SportsTournament.Application.Tournaments.Interfaces;
using SportsTournament.Domain.Tournaments;

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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveAsync(TournamentCacheKeys.List, cancellationToken);

        return tournament.Id;
    }
}