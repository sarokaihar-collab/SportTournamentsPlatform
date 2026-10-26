using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportTournaments.Application.Common.Interfaces;
using SportTournaments.Application.Common.Outbox;
using SportTournaments.Application.Common.Outbox.Interfaces;
using SportTournaments.Application.Tournaments.Interfaces;
using SportTournaments.Infrastructure.Caching;
using SportTournaments.Infrastructure.Persistence;
using SportTournaments.Infrastructure.Persistence.Outbox;
using SportTournaments.Infrastructure.Persistence.Tournaments;

namespace SportTournaments.Infrastructure;

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
            options.InstanceName = "SportTournamentsPlatform:";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddScoped<IOutboxMessageStore, OutboxMessageStore>();

        return services;
    }
}