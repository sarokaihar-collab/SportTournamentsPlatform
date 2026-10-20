using Microsoft.Extensions.DependencyInjection;
using SportsTournament.Application.Tournaments.Commands;
using SportsTournament.Application.Tournaments.Commands.CreateTournament;
using SportsTournament.Application.Tournaments.Commands.OpenTournamentRegistration;
using SportsTournament.Application.Tournaments.Queries;
using SportsTournament.Application.Tournaments.Queries.TournamentDetails;

namespace SportsTournament.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateTournamentHandler, CreateTournamentHandler>();
        services.AddScoped<IOpenTournamentRegistrationHandler, OpenTournamentRegistrationHandler>();
        services.AddScoped<ITournamentDetailsQuery, TournamentDetailsQuery>();
        services.AddScoped<ITournamentListQuery, TournamentListQuery>();

        return services;
    }
}