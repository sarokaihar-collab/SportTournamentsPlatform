using Microsoft.Extensions.Options;
using SportTournaments.Application.Common.Interfaces;
using SportTournaments.Application.Repositories;
using SportTournaments.Application.Tournaments;
using SportTournaments.Application.Tournaments.Queries;

public sealed class TournamentListQuery(ITournamentRepository tournamentRepository, 
    ICacheService cacheService, IOptions<TournamentCacheSettings> cacheSettings) : ITournamentListQuery
{
    public async Task<IReadOnlyList<TournamentListItem>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<List<TournamentListItem>>(TournamentCacheKeys.List, cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        var tournaments = await tournamentRepository.GetAllAsync(cancellationToken);

        var result = tournaments
            .Select(tournament => new TournamentListItem(
                tournament.Id,
                tournament.Name,
                tournament.StartDate,
                tournament.EndDate,
                tournament.MaxTeams,
                tournament.PlayersOnFieldPerTeam,
                tournament.Status.ToString()))
            .ToList();

        await cacheService.SetAsync(TournamentCacheKeys.List, result, cacheSettings.Value.ListTtl, cancellationToken);

        return result;
    }
}