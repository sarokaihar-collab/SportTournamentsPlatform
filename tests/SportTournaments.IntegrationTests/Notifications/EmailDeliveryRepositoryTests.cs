using SportTournaments.NotificationService.Persistence.Entities;
using SportTournaments.NotificationService.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SportTournaments.IntegrationTests.Notifications;

public sealed class EmailDeliveryRepositoryTests(NotificationPostgreSqlFixture fixture)
    : IClassFixture<NotificationPostgreSqlFixture>
{
    [Fact]
    public async Task ClaimBatchAsync_ShouldClaimOnlyDuePendingDeliveries()
    {
        // Arrange
        await fixture.ResetAsync();

        var now = DateTimeOffset.UtcNow;

        var firstDelivery = EmailDelivery.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "first@example.com",
            "Subject 1",
            "Body 1");

        var secondDelivery = EmailDelivery.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "second@example.com",
            "Subject 2",
            "Body 2");

        var futureDelivery = EmailDelivery.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "future@example.com",
            "Subject 3",
            "Body 3");

        futureDelivery.MarkFailed(
            "Temporary SMTP failure.",
            DateTimeOffset.UtcNow.AddHours(1),
            maxAttempts: 5);

        await using (var dbContext = fixture.CreateDbContext())
        {
            dbContext.EmailDeliveries.AddRange(
                firstDelivery,
                secondDelivery,
                futureDelivery);

            await dbContext.SaveChangesAsync();
        }

        IReadOnlyList<EmailDelivery> claimedDeliveries;

        await using (var dbContext = fixture.CreateDbContext())
        {
            var repository = new EmailDeliveryRepository(dbContext);

            claimedDeliveries = await repository.ClaimBatchAsync(
                batchSize: 10,
                lockDuration: TimeSpan.FromMinutes(2));
        }

        Assert.Equal(2, claimedDeliveries.Count);

        Assert.Contains(claimedDeliveries, x => x.Id == firstDelivery.Id);
        Assert.Contains(claimedDeliveries, x => x.Id == secondDelivery.Id);
        Assert.DoesNotContain(claimedDeliveries, x => x.Id == futureDelivery.Id);

        Assert.All(claimedDeliveries, delivery =>
        {
            Assert.NotNull(delivery.LockToken);
            Assert.NotNull(delivery.LockedUntilUtc);
            Assert.True(delivery.LockedUntilUtc > DateTimeOffset.UtcNow);
        });

        await using (var verifyDbContext = fixture.CreateDbContext())
        {
            var persistedFirst = await verifyDbContext.EmailDeliveries
                .SingleAsync(x => x.Id == firstDelivery.Id);

            var persistedSecond = await verifyDbContext.EmailDeliveries
                .SingleAsync(x => x.Id == secondDelivery.Id);

            var persistedFuture = await verifyDbContext.EmailDeliveries
                .SingleAsync(x => x.Id == futureDelivery.Id);

            Assert.NotNull(persistedFirst.LockToken);
            Assert.NotNull(persistedFirst.LockedUntilUtc);

            Assert.NotNull(persistedSecond.LockToken);
            Assert.NotNull(persistedSecond.LockedUntilUtc);

            Assert.Null(persistedFuture.LockToken);
            Assert.Null(persistedFuture.LockedUntilUtc);
        }
    }
}