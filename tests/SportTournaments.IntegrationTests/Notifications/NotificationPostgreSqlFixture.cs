using Microsoft.EntityFrameworkCore;
using Npgsql;
using SportTournaments.NotificationService.Persistence;

namespace SportTournaments.IntegrationTests.Notifications;

public sealed class NotificationPostgreSqlFixture : IAsyncLifetime
{
    private const string TestDatabaseName = "sport_tournaments_notification_service_tests";
    private const string TestConnectionString = "SPORTS_TOURNAMENT_NOTIFICATION_TEST_CONNECTION";


    private readonly DbContextOptions<NotificationDbContext> _dbContextOptions;

    public string ConnectionString { get; }

    public NotificationPostgreSqlFixture()
    {
        var connectionString = Environment.GetEnvironmentVariable(TestConnectionString);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Environment variable " + "'TestConnectionString' " + "is not configured.");
        }

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);

        // Protect the application database from accidental test cleanup.
        if (!string.Equals(connectionStringBuilder.Database, TestDatabaseName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Notification integration tests must use database " + $"'{TestDatabaseName}'.");
        }

        ConnectionString = connectionStringBuilder.ConnectionString;

        _dbContextOptions = new DbContextOptionsBuilder<NotificationDbContext>().UseNpgsql(ConnectionString).Options;
    }

    public NotificationDbContext CreateDbContext()
    {
        return new NotificationDbContext(_dbContextOptions);
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

    public async Task ResetAsync()
    {
        await using var dbContext = CreateDbContext();

        await dbContext.Notifications.ExecuteDeleteAsync();
    }
}