using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsTournament.Application.Tournaments;
using SportsTournament.Infrastructure.Persistence;
using SportsTournament.Infrastructure.Persistence.Tournaments;

namespace SportsTournament.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<PlatformDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ITournamentRepository, TournamentRepository>();

        return services;
    }
}