using SportsTournament.Application.Common;
using SportsTournament.Application.Common.Outbox;
using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Commands.OpenTournamentRegistration;

public sealed class OpenTournamentRegistrationHandler(ITournamentRepository tournamentRepository,
    ICacheService cacheService, IOutboxMessageWriter outboxMessageWriter, IUnitOfWork unitOfWork) : IOpenTournamentRegistrationHandler
{
    public async Task<bool> HandleAsync(OpenTournamentRegistrationCommand command, CancellationToken cancellationToken)
    {
        var tournament = await tournamentRepository.GetByIdForUpdateAsync(command.TournamentId, cancellationToken);

        if (tournament is null)
        {
            return false;
        }

        tournament.OpenRegistration(DateTimeOffset.UtcNow);

        outboxMessageWriter.Enqueue(OutboxMessageTypes.TournamentCacheInvalidationRequested,
            new TournamentCacheInvalidationRequested(InvalidateTournamentList: true, TournamentId: tournament.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveAsync(TournamentCacheKeys.List, cancellationToken);
        await cacheService.RemoveAsync(TournamentCacheKeys.Details(tournament.Id), cancellationToken);

        return true;
    }
}