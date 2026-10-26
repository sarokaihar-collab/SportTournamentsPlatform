using SportsTournament.Application.Common.Interfaces;
using SportsTournament.Application.Outbox.Messages;

namespace SportsTournament.Application.Tournaments;

public sealed class TournamentCacheInvalidationHandler(ICacheService cacheService)
{
    public async Task HandleAsync(TournamentListCacheInvalidationRequested message, CancellationToken cancellationToken = default)
    {
        await RemoveRequiredAsync(TournamentCacheKeys.List, cancellationToken);
    }

    public async Task HandleAsync(TournamentDetailsCacheInvalidationRequested message, CancellationToken cancellationToken = default)
    {
        if (message.TournamentId == Guid.Empty)
        {
            throw new InvalidOperationException("TournamentId cann't be empty.");
        }

        var cacheKey = TournamentCacheKeys.Details(message.TournamentId);

        await RemoveRequiredAsync(cacheKey, cancellationToken);
    }

    private async Task RemoveRequiredAsync(string key, CancellationToken cancellationToken)
    {
        var removed = await cacheService.RemoveAsync(key, cancellationToken);

        if (!removed)
        {
            throw new InvalidOperationException($"Failed to invalidate cache key '{key}'.");
        }
    }
}