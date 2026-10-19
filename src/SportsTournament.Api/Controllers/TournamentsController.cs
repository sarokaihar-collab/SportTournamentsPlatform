using Microsoft.AspNetCore.Mvc;
using SportsTournament.Api.Contracts.Tournaments;
using SportsTournament.Application.Common;
using SportsTournament.Application.Tournaments.Commands;
using SportsTournament.Application.Tournaments.Queries;

namespace SportsTournament.Api.Controllers;

[ApiController]
[Route("api/tournaments")]
public sealed class TournamentsController(
    ITournamentListQuery tournamentListQuery,
    ICreateTournamentHandler createTournamentHandler,
    ICurrentUserService currentUserService,
    ITournamentDetailsQuery tournamentDetailsQuery) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTournamentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTournamentCommand(
            request.Name,
            currentUserService.GetUser(),
            request.RegistrationDeadline,
            request.StartDate,
            request.EndDate,
            request.MaxTeams);

        var tournamentId = await createTournamentHandler.HandleAsync(command, cancellationToken);

        return Created($"/api/tournaments/{tournamentId}", new { id = tournamentId });
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TournamentListItem>), 200)]
    public async Task<ActionResult<IReadOnlyList<TournamentListItem>>> Get(CancellationToken cancellationToken)
    {
        var tournaments = await tournamentListQuery.ExecuteAsync(cancellationToken);

        return Ok(tournaments);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TournamentListItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TournamentListItem>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tournament = await tournamentDetailsQuery.ExecuteAsync(id, cancellationToken);

        if (tournament is null)
        {
            return NotFound();
        }

        return Ok(tournament);
    }
}