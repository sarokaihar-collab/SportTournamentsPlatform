using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTournament.Domain;
using SportsTournament.Domain.Teams;
using SportsTournament.Domain.Tournaments;

namespace SportsTournament.Infrastructure.Persistence.Configurations;

public sealed class TournamentRegistrationConfiguration : IEntityTypeConfiguration<TournamentRegistration>
{
    public void Configure(EntityTypeBuilder<TournamentRegistration> builder)
    {
        builder.ToTable("tournament_registrations");

        builder.HasKey(registration => registration.Id);

        builder.Property(registration => registration.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(registration => registration.TournamentId).HasColumnName("tournament_id").IsRequired();
        builder.Property(registration => registration.TeamId).HasColumnName("team_id").IsRequired();
        builder.Property(registration => registration.Status).HasColumnName("status").HasColumnType("integer").IsRequired();
        builder.Property(registration => registration.RegisteredAtUtc).HasColumnName("registered_at_utc").HasColumnType("timestamp with time zone").IsRequired();

        builder.HasOne<Tournament>().WithMany().HasForeignKey(registration => registration.TournamentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Team>().WithMany().HasForeignKey(registration => registration.TeamId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(registration => new
        {
            registration.TournamentId,
            registration.TeamId
        }).IsUnique();
    }
}