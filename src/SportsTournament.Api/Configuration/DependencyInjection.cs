using SportsTournament.Application.Tournaments;

namespace SportsTournament.Api.Configuration;

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

        return services;
    }
}