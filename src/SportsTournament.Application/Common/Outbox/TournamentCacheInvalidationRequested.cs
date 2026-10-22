namespace SportsTournament.Application.Common.Outbox;

public sealed record TournamentCacheInvalidationRequested(bool InvalidateTournamentList, Guid? TournamentId);