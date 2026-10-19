namespace SportsTournament.Api.Contracts.Tournaments;

public sealed record CreateTournamentRequest(
    string Name,
    DateTimeOffset RegistrationDeadline,
    DateOnly StartDate,
    DateOnly EndDate,
    int MaxTeams);