namespace SportsTournament.Application.Common.Outbox;

public static class OutboxMessageTypes
{
    public const string TournamentListCacheInvalidationRequested = "tournament-list-cache-invalidation-requested.v1";

    public const string TournamentDetailsCacheInvalidationRequested = "tournament-details-cache-invalidation-requested.v1";
}