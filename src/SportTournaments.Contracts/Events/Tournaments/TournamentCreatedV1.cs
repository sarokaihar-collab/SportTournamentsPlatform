namespace SportTournaments.Contracts.Events.Tournaments;

public sealed record TournamentCreatedV1(Guid EventId, Guid TournamentId, Guid OrganizerId, string TournamentName, DateTimeOffset OccurredAtUtc);