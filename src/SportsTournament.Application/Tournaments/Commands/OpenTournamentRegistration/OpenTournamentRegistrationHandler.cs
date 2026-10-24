using SportsTournament.Application.Common.Interfaces;
using SportsTournament.Application.Common.Outbox;
using SportsTournament.Application.Outbox.Messages;
using SportsTournament.Application.Common.Outbox.Interfaces;
using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Commands.OpenTournamentRegistration;
using SportsTournament.Application.Tournaments.Interfaces;

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

        outboxMessageWriter.Enqueue(OutboxMessageTypes.TournamentListCacheInvalidationRequested, new TournamentListCacheInvalidationRequested());
        outboxMessageWriter.Enqueue(OutboxMessageTypes.TournamentDetailsCacheInvalidationRequested, new TournamentDetailsCacheInvalidationRequested(tournament.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveAsync(TournamentCacheKeys.List, cancellationToken);
        await cacheService.RemoveAsync(TournamentCacheKeys.Details(tournament.Id), cancellationToken);

        return true;
    }
}