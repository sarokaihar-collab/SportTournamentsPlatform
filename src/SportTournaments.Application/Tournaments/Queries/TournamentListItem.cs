namespace SportTournaments.Application.Tournaments.Queries;

public sealed record TournamentListItem(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    int MaxTeams,
    int PlayersOnFieldPerTeam,
    string Status);