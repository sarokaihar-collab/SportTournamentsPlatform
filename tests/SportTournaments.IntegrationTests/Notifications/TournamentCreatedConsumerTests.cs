using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SportTournaments.Contracts.Events.Tournaments;
using SportTournaments.NotificationService.Consumers;
using SportTournaments.NotificationService.Persistence;
using SportTournaments.NotificationService.Persistence.Entities;

namespace SportTournaments.IntegrationTests.Notifications;

public sealed class TournamentCreatedConsumerTests(NotificationPostgreSqlFixture fixture) : IClassFixture<NotificationPostgreSqlFixture>
{
    [Fact]
    public async Task Consume_ShouldPersistNotification()
    {
        // Arrange
        await fixture.ResetAsync();

        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddDbContext<NotificationDbContext>(options => options.UseNpgsql(fixture.ConnectionString))
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<TournamentCreatedConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var message = new TournamentCreatedV1(
            EventId: Guid.NewGuid(),
            TournamentId: Guid.NewGuid(),
            OrganizerId: Guid.NewGuid(),
            TournamentName: "Integration test tournament",
            OccurredAtUtc: DateTimeOffset.UtcNow,
            OrganizerEmail: "development.user@example.com");

        var consumerHarness = harness.GetConsumerHarness<TournamentCreatedConsumer>();

        // Act
        await harness.Bus.Publish(message);

        // Wait until the consumer actually processes the message.
        var consumed = await consumerHarness.Consumed.Any<TournamentCreatedV1>(x => x.Context.Message.EventId == message.EventId);

        // Assert
        Assert.True(consumed);

        await using var dbContext = fixture.CreateDbContext();

        var notification = await dbContext.Notifications.SingleAsync();

        Assert.Equal(message.EventId, notification.EventId);
        Assert.Equal(message.OrganizerId, notification.RecipientUserId);
        Assert.Equal(message.TournamentId, notification.TournamentId);
        Assert.Equal("TournamentCreated", notification.NotificationType);
        Assert.Equal("Турнир создан", notification.Title);
        Assert.Equal($"Турнир «{message.TournamentName}» создан.", notification.Message);
        Assert.Equal(message.OccurredAtUtc, notification.EventOccurredAtUtc, TimeSpan.FromMilliseconds(1));

        var emailDeliveries = await dbContext.EmailDeliveries
            .AsNoTracking()
            .Where(x => x.EventId == message.EventId)
            .ToListAsync();

        var emailDelivery = Assert.Single(emailDeliveries);

        Assert.Equal(message.EventId, emailDelivery.EventId);
        Assert.Equal(message.OrganizerId, emailDelivery.RecipientUserId);
        Assert.Equal("development.user@example.com", emailDelivery.RecipientEmail);
        Assert.Equal("Турнир создан", emailDelivery.Subject);
        Assert.Equal($"Турнир «{message.TournamentName}» создан.", emailDelivery.Body);
        Assert.Equal(EmailDeliveryStatus.Pending, emailDelivery.Status);
        Assert.Equal(0, emailDelivery.AttemptCount);
        Assert.Null(emailDelivery.SentAtUtc);
    }

    [Fact]
    public async Task Consume_ShouldIgnoreDuplicateEventDelivery()
    {
        // Arrange
        await fixture.ResetAsync();

        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddDbContext<NotificationDbContext>(options => options.UseNpgsql(fixture.ConnectionString))
            .AddMassTransitTestHarness(x =>
            {
                x.SetTestTimeouts(testTimeout: TimeSpan.FromSeconds(15), testInactivityTimeout: TimeSpan.FromSeconds(3));

                x.AddConsumer<TournamentCreatedConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var consumerHarness = harness.GetConsumerHarness<TournamentCreatedConsumer>();

        var message = new TournamentCreatedV1(
            EventId: Guid.NewGuid(),
            TournamentId: Guid.NewGuid(),
            OrganizerId: Guid.NewGuid(),
            TournamentName: "Idempotency test tournament",
            OccurredAtUtc: DateTimeOffset.UtcNow,
            OrganizerEmail: "development.user@example.com");

        // Act: first delivery
        await harness.Bus.Publish(message);

        var firstDelivery = await consumerHarness.Consumed.SelectAsync<TournamentCreatedV1>().FirstOrDefault();

        Assert.NotNull(firstDelivery);

        // Verify that the first delivery created a notification.
        await using (var dbContext = fixture.CreateDbContext())
        {
            var count = await dbContext.Notifications.CountAsync(x => x.EventId == message.EventId);

            Assert.Equal(1, count);
        }

        // Act: publish the same business event again.
        await harness.Bus.Publish(message);

        // Wait specifically for the second transport delivery.
        // Its MessageId differs, although EventId remains the same.
        var duplicateConsumed = await consumerHarness.Consumed
            .Any<TournamentCreatedV1>(x => x.Context.Message.EventId == message.EventId  && x.Context.MessageId != firstDelivery.Context.MessageId);

        // Assert
        Assert.True(duplicateConsumed);

        await using var verificationContext = fixture.CreateDbContext();

        var finalCount = await verificationContext.Notifications.CountAsync(x => x.EventId == message.EventId);

        Assert.Equal(1, finalCount);
    }

    [Fact]
    public async Task Consume_ShouldPublishFault_WhenDatabaseErrorIsNotDuplicateEventId()
    {
        await fixture.ResetAsync();

        try
        {
            // Arrange: force an unrelated PostgreSQL constraint violation.
            await using (var schemaContext = fixture.CreateDbContext())
            {
                await schemaContext.Database.ExecuteSqlRawAsync(
                    """ALTER TABLE notifications ADD CONSTRAINT ck_notifications_test_forced_failure CHECK (1 = 0);""");
            }

            await using var provider = new ServiceCollection()
                .AddLogging()
                .AddDbContext<NotificationDbContext>(options => options.UseNpgsql(fixture.ConnectionString))
                .AddMassTransitTestHarness(x =>
                {
                    x.SetTestTimeouts(testTimeout: TimeSpan.FromSeconds(15), testInactivityTimeout: TimeSpan.FromSeconds(3));

                    x.AddConsumer<TournamentCreatedConsumer>();
                })
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var message = new TournamentCreatedV1(
                EventId: Guid.NewGuid(),
                TournamentId: Guid.NewGuid(),
                OrganizerId: Guid.NewGuid(),
                TournamentName: "Database failure test",
                OccurredAtUtc: DateTimeOffset.UtcNow,
                OrganizerEmail: "development.user@example.com");

            // Act
            await harness.Bus.Publish(message);

            // Assert: MassTransit observed the consumer failure.
            var faultPublished = await harness.Published.Any<Fault<TournamentCreatedV1>>(x => x.Context.Message.Message.EventId == message.EventId);

            Assert.True(faultPublished);

            await using var verificationContext = fixture.CreateDbContext();

            var notificationCount = await verificationContext.Notifications.CountAsync(x => x.EventId == message.EventId);

            Assert.Equal(0, notificationCount);
        }
        finally
        {
            await using var cleanupContext = fixture.CreateDbContext();

            await cleanupContext.Database.ExecuteSqlRawAsync("""ALTER TABLE notifications DROP CONSTRAINT IF EXISTS ck_notifications_test_forced_failure;""");
        }
    }
}