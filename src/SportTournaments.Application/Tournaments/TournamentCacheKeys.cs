namespace SportTournaments.Application.Tournaments;

public static class TournamentCacheKeys
{
    public const string List = "tournaments:list:v1";

    public static string Details(Guid tournamentId) =>  $"tournaments:details:v1:{tournamentId:D}";
}