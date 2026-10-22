using SportsTournament.Application.Tournaments;
using SportsTournament.Domain.Tournaments;

namespace SportsTournament.UnitTests.Common.Fakes;

public sealed class FakeTournamentRepository : ITournamentRepository
{
    public List<Tournament> Tournaments { get; } = [];

    public int GetAllCallCount { get; private set; }

    public int GetByIdCallCount { get; private set; }

    public int GetByIdForUpdateCallCount { get; private set; }

    public int CreateCallCount { get; private set; }

    public int UpdateCallCount { get; private set; }

    public Task<IReadOnlyList<Tournament>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        GetAllCallCount++;

        IReadOnlyList<Tournament> result = Tournaments.ToArray();

        return Task.FromResult(result);
    }

    public Task<Tournament?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        GetByIdCallCount++;

        return Task.FromResult(Tournaments.SingleOrDefault(tournament => tournament.Id == id));
    }

    public Task<Tournament?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        GetByIdForUpdateCallCount++;

        return Task.FromResult(Tournaments.SingleOrDefault(tournament => tournament.Id == id));
    }

    void ITournamentRepository.Add(Tournament tournament)
    {
        CreateCallCount++;
        Tournaments.Add(tournament);
    }
}