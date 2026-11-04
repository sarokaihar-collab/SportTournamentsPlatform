using Microsoft.EntityFrameworkCore;
using SportTournaments.Domain.Common;
using SportTournaments.Infrastructure.Persistence;

public sealed class UserRepository(PlatformDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }
}