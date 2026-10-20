namespace SportsTournament.Application.Tournaments.Commands.CreateTournament;

public interface ICreateTournamentHandler
{
    Task<Guid> HandleAsync(CreateTournamentCommand command, CancellationToken cancellationToken);
}