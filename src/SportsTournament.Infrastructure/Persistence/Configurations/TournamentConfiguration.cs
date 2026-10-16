using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTournament.Domain.Common;
using SportsTournament.Domain.Enums;
using SportsTournament.Domain.Tournaments;

namespace SportsTournament.Infrastructure.Persistence.Configurations;

public sealed class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("tournaments", table =>
        {
            table.HasCheckConstraint("ck_tournaments_max_teams", "\"max_teams\" >= 3");
            table.HasCheckConstraint("ck_tournaments_date_range", "\"start_date\" <= \"end_date\"");
        });

        builder.HasKey(tournament => tournament.Id);
        builder.HasOne<User>().WithMany().HasForeignKey(tournament => tournament.OrganizerId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(tournament => tournament.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(tournament => tournament.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(tournament => tournament.OrganizerId).HasColumnName("organizer_id").IsRequired();
        builder.Property(tournament => tournament.RegistrationDeadlineUtc).HasColumnName("registration_deadline_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(tournament => tournament.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
        builder.Property(tournament => tournament.EndDate).HasColumnName("end_date").HasColumnType("date").IsRequired();
        builder.Property(tournament => tournament.MaxTeams).HasColumnName("max_teams").IsRequired();
        builder.Property(tournament => tournament.Status).HasColumnName("status").HasColumnType("integer").IsRequired();
        builder.Property(tournament => tournament.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
    }
}