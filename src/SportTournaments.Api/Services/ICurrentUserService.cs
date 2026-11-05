namespace SportTournaments.Application.Common;

public interface ICurrentUserService
{
    Task<Guid> GetUserAsync(CancellationToken cancellationToken = default);
}