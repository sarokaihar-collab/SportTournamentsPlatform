using Microsoft.EntityFrameworkCore;
using SportTournaments.Infrastructure.Persistence;

namespace SportTournaments.IntegrationTests.Infrastructure;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly string _connectionString;

    public PostgreSqlFixture()
    {
        _connectionString = Environment.GetEnvironmentVariable("SPORTS_TOURNAMENT_TEST_CONNECTION")
            ?? throw new InvalidOperationException("Environment variable " + "'SPORTS_TOURNAMENT_TEST_CONNECTION' is not configured.");
    }

    public async Task InitializeAsync()
    {
        await using var dbContext = CreateDbContext();

        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public PlatformDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseNpgsql(_connectionString).Options;

        return new PlatformDbContext(options);
    }
}

[CollectionDefinition(nameof(PostgreSqlCollection))]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
}