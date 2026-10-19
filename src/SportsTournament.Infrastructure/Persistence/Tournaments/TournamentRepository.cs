using Microsoft.EntityFrameworkCore;
using SportsTournament.Application.Tournaments;
using SportsTournament.Domain.Tournaments;

namespace SportsTournament.Infrastructure.Persistence.Tournaments;

public sealed class TournamentRepository(PlatformDbContext dbContext) : ITournamentRepository
{
    public async Task<IReadOnlyList<Tournament>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Tournaments
            .AsNoTracking()
            .OrderBy(tournament => tournament.StartDate)
            .ThenBy(tournament => tournament.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(Tournament tournament, CancellationToken cancellationToken)
    {
        dbContext.Tournaments.Add(tournament);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Tournament?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Tournaments
            .AsNoTracking()
            .SingleOrDefaultAsync(tournament => tournament.Id == id, cancellationToken);
    }
}