using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportTournaments.Domain.Common;
using SportTournaments.Domain.Teams;

namespace SportTournaments.Infrastructure.Persistence.Configurations;

public sealed class TeamManagerAssignmentConfiguration : IEntityTypeConfiguration<TeamManagerAssignment>
{
    public void Configure(EntityTypeBuilder<TeamManagerAssignment> builder)
    {
        builder.ToTable("team_manager_assignments");

        builder.HasKey(assignment => assignment.Id);

        builder.Property(assignment => assignment.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(assignment => assignment.TeamId).HasColumnName("team_id").IsRequired();
        builder.Property(assignment => assignment.ManagerId).HasColumnName("manager_id").IsRequired();
        builder.Property(assignment => assignment.AssignedAtUtc).HasColumnName("assigned_at_utc").HasColumnType("timestamp with time zone").IsRequired();

        builder.HasOne<Team>().WithMany().HasForeignKey(assignment => assignment.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(assignment => assignment.ManagerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(assignment => assignment.TeamId).IsUnique();
        builder.HasIndex(assignment => assignment.ManagerId).IsUnique();
    }
}