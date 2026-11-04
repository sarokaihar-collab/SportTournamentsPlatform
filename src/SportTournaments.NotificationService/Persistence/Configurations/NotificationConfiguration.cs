using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportTournaments.NotificationService.Persistence.Entities;

namespace SportTournaments.NotificationService.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.EventId).IsRequired();
        builder.HasIndex(x => x.EventId).IsUnique().HasDatabaseName("ux_notifications_event_id");
        builder.Property(x => x.RecipientUserId).IsRequired();
        builder.Property(x => x.TournamentId);
        builder.Property(x => x.NotificationType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.EventOccurredAtUtc).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ReadAtUtc);
    }
}