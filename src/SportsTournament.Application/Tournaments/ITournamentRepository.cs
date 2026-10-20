using SportsTournament.Domain.Tournaments;

namespace SportsTournament.Application.Tournaments;

public interface ITournamentRepository
{
    Task<Tournament?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Tournament>> GetAllAsync(CancellationToken cancellationToken);

    Task CreateAsync(Tournament tournament, CancellationToken cancellationToken);

    Task<Tournament?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task UpdateAsync(Tournament tournament, CancellationToken cancellationToken);
}