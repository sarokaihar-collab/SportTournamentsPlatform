using SportsTournament.Domain.Tournaments;

namespace SportsTournament.Application.Tournaments.Commands
{
    public sealed class CreateTournamentHandler(ITournamentRepository tournamentRepository) : ICreateTournamentHandler
    {
        public async Task<Guid> HandleAsync(CreateTournamentCommand command, CancellationToken cancellationToken)
        {
            var tournament = new Tournament(
                command.Name,
                command.OrganizerId,
                command.RegistrationDeadline,
                command.StartDate,
                command.EndDate,
                command.MaxTeams);

            await tournamentRepository.CreateAsync(tournament, cancellationToken);

            return tournament.Id;
        }
    }
}