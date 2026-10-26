using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportTournaments.Application.Common.Outbox;
using SportTournaments.Application.Tournaments;
using SportTournaments.Application.Tournaments.Commands;
using SportTournaments.Application.Tournaments.Interfaces;
using SportTournaments.Application.Tournaments.Queries;

namespace SportTournaments.Application;

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
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        return services;
    }
}