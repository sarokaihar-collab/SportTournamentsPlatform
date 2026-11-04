using Microsoft.EntityFrameworkCore;
using SportTournaments.Application.Repositories;
using SportTournaments.Domain.Tournaments;

namespace SportTournaments.Infrastructure.Persistence.Tournaments;

public sealed class TournamentRepository(PlatformDbContext dbContext) : ITournamentRepository
{
    public void Add(Tournament tournament)
    {
        ArgumentNullException.ThrowIfNull(tournament);
        dbContext.Tournaments.Add(tournament);
    }

    public async Task<IReadOnlyList<Tournament>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Tournaments
            .AsNoTracking()
            .OrderBy(tournament => tournament.StartDate)
            .ThenBy(tournament => tournament.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tournament?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Tournaments.AsNoTracking().SingleOrDefaultAsync(tournament => tournament.Id == id, cancellationToken);
    }

    public async Task<Tournament?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Tournaments.AsTracking().SingleOrDefaultAsync(tournament => tournament.Id == id, cancellationToken);
    }
}