using Microsoft.EntityFrameworkCore;
using SportsTournament.Application.Common.Interfaces;
using SportsTournament.Application.Common.Outbox;
using SportsTournament.Application.Outbox.Messages;
using SportsTournament.Application.Tournaments;
using SportsTournament.Domain.Tournaments;
using SportsTournament.Infrastructure.Persistence.Outbox;
using SportsTournament.Infrastructure.Persistence.Tournaments;
using System.Text.Json;

namespace SportsTournament.IntegrationTests.Infrastructure;

[Collection(nameof(PostgreSqlCollection))]
public sealed class TransactionalOutboxTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task SaveChangesAsync_WhenTournamentAndOutboxAdded_PersistsBoth()
    {
        // Arrange
        var tournament = new Tournament(
            $"Outbox Integration Test {Guid.NewGuid()}",
            Guid.NewGuid(),
            new DateTimeOffset(
                2026, 11, 6, 18, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 11, 7),
            new DateOnly(2026, 11, 8),
            8,
            11);

        var payload = new TournamentDetailsCacheInvalidationRequested(tournament.Id);

        // Act: both changes use the same DbContext and Unit of Work.
        await using (var dbContext = fixture.CreateDbContext())
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO public.users (id, display_name, created_at_utc)
                VALUES (
                    {tournament.OrganizerId},
                    {"Transactional Outbox test organizer"},
                    {DateTime.UtcNow}
                )
                ON CONFLICT (id) DO NOTHING;
                """);

            var repository = new TournamentRepository(dbContext);
            var outboxWriter = new OutboxMessageWriter(dbContext);
            IUnitOfWork unitOfWork = dbContext;

            repository.Add(tournament);

            outboxWriter.Enqueue(OutboxMessageTypes.TournamentDetailsCacheInvalidationRequested, payload);

            await unitOfWork.SaveChangesAsync();
        }

        // Assert: verify persisted data using a fresh DbContext.
        await using var verificationContext = fixture.CreateDbContext();

        var persistedTournament = await verificationContext.Tournaments.AsNoTracking().SingleOrDefaultAsync(item => item.Id == tournament.Id);

        Assert.NotNull(persistedTournament);
        Assert.Equal(tournament.Name, persistedTournament.Name);

        var outboxMessages = await verificationContext.OutboxMessages
            .AsNoTracking()
            .Where(message => message.MessageType == OutboxMessageTypes.TournamentDetailsCacheInvalidationRequested)
            .ToListAsync();

        var matchingMessage = Assert.Single(outboxMessages.Where(message => message.Payload.Contains(tournament.Id.ToString())));

        var persistedPayload = JsonSerializer.Deserialize<TournamentDetailsCacheInvalidationRequested>(
                matchingMessage.Payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(persistedPayload);
        Assert.Equal(tournament.Id, persistedPayload.TournamentId);
        Assert.Null(matchingMessage.ProcessedAtUtc);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenTournamentOrOutboxNotAdded_PersistsNothing()
    {
        // Arrange
        var tournament = new Tournament(
            $"Outbox Integration Test {Guid.NewGuid()}",
            Guid.NewGuid(),
            new DateTimeOffset(
                2026, 11, 6, 18, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 11, 7),
            new DateOnly(2026, 11, 8),
            8,
            11);

        var payload = new TournamentListCacheInvalidationRequested();

        await using (var dbContext = fixture.CreateDbContext())
        {
            var repository = new TournamentRepository(dbContext);
            var outboxWriter = new OutboxMessageWriter(dbContext);
            IUnitOfWork unitOfWork = dbContext;

            repository.Add(tournament);

            outboxWriter.Enqueue(new string('x', 201), payload);

            await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync());
        }

        await using (var verificationContext = fixture.CreateDbContext())
        {
            var tournamentExists = await verificationContext.Tournaments.AnyAsync(t => t.Id == tournament.Id);

            Assert.False(tournamentExists);

            var payloads = await verificationContext.OutboxMessages.Select(message => message.Payload).ToListAsync();

            Assert.DoesNotContain(payloads, storedPayload => storedPayload.Contains(tournament.Id.ToString()));
        }
    }
}