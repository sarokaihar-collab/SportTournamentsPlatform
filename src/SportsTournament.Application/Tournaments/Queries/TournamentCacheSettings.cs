namespace SportsTournament.Application.Tournaments;

public sealed class TournamentCacheSettings
{
    public const string SectionName = "Caching:Tournaments";

    public TimeSpan ListTtl { get; set; }

    public TimeSpan DetailsTtl { get; set; }
}