using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsTournament.Application.Common.Interfaces;
using SportsTournament.Application.Common.Outbox;
using SportsTournament.Application.Common.Outbox.Interfaces;
using SportsTournament.Application.Tournaments.Interfaces;
using SportsTournament.Infrastructure.Caching;
using SportsTournament.Infrastructure.Persistence;
using SportsTournament.Infrastructure.Persistence.Outbox;
using SportsTournament.Infrastructure.Persistence.Tournaments;

namespace SportsTournament.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        var redisConnectionString = configuration.GetConnectionString("Redis") 
            ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");


        services.AddDbContext<PlatformDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ITournamentRepository, TournamentRepository>();
        services.AddScoped<IOutboxMessageWriter, OutboxMessageWriter>();

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<PlatformDbContext>());

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "SportsTournamentPlatform:";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddScoped<IOutboxMessageStore, OutboxMessageStore>();

        return services;
    }
}