using Microsoft.EntityFrameworkCore;
using SportTournaments.Domain.Common;
using SportTournaments.Infrastructure.Persistence;

public sealed class UserRepository(PlatformDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByExternalIdentityAsync(string issuer, string subject, CancellationToken cancellationToken = default)
    {
        return dbContext.UserIdentities.AsNoTracking()
            .Where(identity => identity.Issuer == issuer && identity.Subject == subject)
            .Join(dbContext.Users.AsNoTracking(), identity => identity.UserId, user => user.Id, (_, user) => user)
            .SingleOrDefaultAsync(cancellationToken);
    }
}