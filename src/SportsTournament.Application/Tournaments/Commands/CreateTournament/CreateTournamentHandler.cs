using SportsTournament.Application.Common;
using SportsTournament.Application.Common.Outbox;
using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Commands.CreateTournament;
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

        outboxMessageWriter.Enqueue(OutboxMessageTypes.TournamentCacheInvalidationRequested,
            new TournamentCacheInvalidationRequested(InvalidateTournamentList: true, TournamentId: null));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveAsync(TournamentCacheKeys.List, cancellationToken);

        return tournament.Id;
    }
}