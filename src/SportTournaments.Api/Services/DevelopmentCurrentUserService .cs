using SportTournaments.Application.Common;

public sealed class DevelopmentCurrentUserService : ICurrentUserService
{
    public static readonly Guid MockUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public Guid GetUser() => MockUserId;
}