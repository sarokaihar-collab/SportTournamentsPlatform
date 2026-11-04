using Microsoft.Extensions.Options;
using SportTournaments.Application.Common.Interfaces;
using SportTournaments.Application.Repositories;
using SportTournaments.Application.Tournaments;
using SportTournaments.Application.Tournaments.Interfaces;
using SportTournaments.Application.Tournaments.Queries;

public sealed class TournamentDetailsQuery(ITournamentRepository tournamentRepository, 
    ICacheService cacheService, IOptions<TournamentCacheSettings> cacheSettings) : ITournamentDetailsQuery
{
    public async Task<TournamentListItem?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = TournamentCacheKeys.Details(id);

        var cached = await cacheService.GetAsync<TournamentListItem>(cacheKey, cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        var tournament = await tournamentRepository.GetByIdAsync(id, cancellationToken);

        if (tournament is null)
        {
            return null;
        }

        var result = new TournamentListItem(
            tournament.Id,
            tournament.Name,
            tournament.StartDate,
            tournament.EndDate,
            tournament.MaxTeams,
            tournament.PlayersOnFieldPerTeam,
            tournament.Status.ToString());

        await cacheService.SetAsync(cacheKey, result, cacheSettings.Value.DetailsTtl, cancellationToken);

        return result;
    }
}