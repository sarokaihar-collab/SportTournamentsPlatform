namespace SportsTournament.Application.Tournaments.Commands.CreateTournament;

public sealed record CreateTournamentCommand(
    string Name,
    Guid OrganizerId,
    DateTimeOffset RegistrationDeadline,
    DateOnly StartDate,
    DateOnly EndDate,
    int MaxTeams,
    int PlayersOnFieldPerTeam);