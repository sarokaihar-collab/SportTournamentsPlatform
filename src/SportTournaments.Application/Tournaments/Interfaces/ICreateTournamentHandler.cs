using SportTournaments.Application.Tournaments.Commands.CreateTournament;

namespace SportTournaments.Application.Tournaments.Interfaces;

public interface ICreateTournamentHandler
{
    Task<Guid> HandleAsync(CreateTournamentCommand command, CancellationToken cancellationToken);
}