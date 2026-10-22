using SportsTournament.Domain.Tournaments;

namespace SportsTournament.Application.Tournaments;

public interface ITournamentRepository
{
    Task<Tournament?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Tournament>> GetAllAsync(CancellationToken cancellationToken);

    void Add(Tournament tournament);

    Task<Tournament?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
}