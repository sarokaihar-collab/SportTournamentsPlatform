using Microsoft.EntityFrameworkCore;
using SportTournaments.NotificationService.Persistence.Entities;

namespace SportTournaments.NotificationService.Persistence;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<EmailDelivery> EmailDeliveries => Set<EmailDelivery>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}