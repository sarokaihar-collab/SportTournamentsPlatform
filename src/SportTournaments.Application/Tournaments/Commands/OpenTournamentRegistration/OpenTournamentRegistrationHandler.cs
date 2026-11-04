using SportTournaments.Application.Common.Interfaces;
using SportTournaments.Application.Common.Outbox;
using SportTournaments.Application.Outbox.Messages;
using SportTournaments.Application.Common.Outbox.Interfaces;
using SportTournaments.Application.Tournaments;
using SportTournaments.Application.Tournaments.Commands.OpenTournamentRegistration;
using SportTournaments.Application.Tournaments.Interfaces;
using SportTournaments.Application.Repositories;

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