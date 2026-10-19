namespace SportsTournament.Application.Tournaments.Commands;

public interface ICreateTournamentHandler
{
    Task<Guid> HandleAsync(CreateTournamentCommand command, CancellationToken cancellationToken);
}