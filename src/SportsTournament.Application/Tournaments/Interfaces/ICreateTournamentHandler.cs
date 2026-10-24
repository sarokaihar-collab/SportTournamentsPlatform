using SportsTournament.Application.Tournaments.Commands.CreateTournament;

namespace SportsTournament.Application.Tournaments.Interfaces;

public interface ICreateTournamentHandler
{
    Task<Guid> HandleAsync(CreateTournamentCommand command, CancellationToken cancellationToken);
}