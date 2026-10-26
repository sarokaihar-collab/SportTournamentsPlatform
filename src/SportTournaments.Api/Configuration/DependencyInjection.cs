using SportTournaments.Application.Common.Outbox;
using SportTournaments.Application.Tournaments;

namespace SportTournaments.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<TournamentCacheSettings>()
            .Bind(configuration.GetSection(TournamentCacheSettings.SectionName))
            .Validate(settings => settings.ListTtl > TimeSpan.Zero, "Tournament list cache TTL must be greater than zero.")
            .Validate(settings => settings.DetailsTtl > TimeSpan.Zero, "Tournament details cache TTL must be greater than zero.")
            .ValidateOnStart();

        services.AddOptions<OutboxProcessingSettings>()
            .Bind(configuration.GetSection(OutboxProcessingSettings.SectionName))
            .Validate(settings => settings.BatchSize > 0 && settings.LockDuration > TimeSpan.Zero &&
                    settings.InitialRetryDelay > TimeSpan.Zero && settings.MaxRetryDelay >= settings.InitialRetryDelay &&
                    settings.PollingInterval > TimeSpan.Zero, "Invalid Outbox processing settings.")
            .ValidateOnStart();

        return services;
    }
}