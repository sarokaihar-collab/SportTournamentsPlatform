using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsTournament.Application.Common.Outbox;
using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Commands;
using SportsTournament.Application.Tournaments.Interfaces;
using SportsTournament.Application.Tournaments.Queries;

namespace SportsTournament.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateTournamentHandler, CreateTournamentHandler>();
        services.AddScoped<IOpenTournamentRegistrationHandler, OpenTournamentRegistrationHandler>();
        services.AddScoped<ITournamentDetailsQuery, TournamentDetailsQuery>();
        services.AddScoped<ITournamentListQuery, TournamentListQuery>();

        services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher>();
        services.AddScoped<TournamentCacheInvalidationHandler>();
        services.AddScoped<OutboxProcessor>();

        return services;
    }
}