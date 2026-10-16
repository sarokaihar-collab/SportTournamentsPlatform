using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsTournament.Domain.Teams;

namespace SportsTournament.Infrastructure.Persistence.Configurations;

public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");

        builder.HasKey(team => team.Id);

        builder.Property(team => team.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(team => team.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(team => team.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
    }
}