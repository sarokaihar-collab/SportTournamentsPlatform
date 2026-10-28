using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportTournaments.NotificationService.Persistence.Entities;

namespace SportTournaments.NotificationService.Persistence.Configurations;

public sealed class EmailDeliveryConfiguration
    : IEntityTypeConfiguration<EmailDelivery>
{
    public void Configure(EntityTypeBuilder<EmailDelivery> builder)
    {
        builder.ToTable("email_deliveries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.EventId).HasColumnName("event_id").IsRequired();
        builder.Property(x => x.RecipientUserId).HasColumnName("recipient_user_id").IsRequired();
        builder.Property(x => x.RecipientEmail).HasColumnName("recipient_email").HasMaxLength(254).IsRequired();
        builder.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasColumnName("body").HasColumnType("text").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(x => x.AttemptCount).HasColumnName("attempt_count").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.NextAttemptAtUtc).HasColumnName("next_attempt_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.SentAtUtc).HasColumnName("sent_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
        builder.HasIndex(x => new { x.EventId, x.RecipientUserId }).IsUnique().HasDatabaseName("ux_email_deliveries_event_recipient");
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc }).HasDatabaseName("ix_email_deliveries_status_next_attempt_at_utc");
    }
}