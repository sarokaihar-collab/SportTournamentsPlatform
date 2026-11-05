using SportTournaments.Domain.Common;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByExternalIdentityAsync(string issuer, string subject, CancellationToken cancellationToken = default); }