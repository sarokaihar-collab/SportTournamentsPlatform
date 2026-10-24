using Microsoft.EntityFrameworkCore;
using SportsTournament.Application.Common.Interfaces;
using SportsTournament.Domain;
using SportsTournament.Domain.Common;
using SportsTournament.Domain.Teams;
using SportsTournament.Domain.Tournaments;
using SportsTournament.Infrastructure.Persistence.Outbox;

namespace SportsTournament.Infrastructure.Persistence;

public sealed class PlatformDbContext : DbContext, IUnitOfWork
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
        : base(options)
    {

    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<TeamManagerAssignment> TeamManagerAssignments => Set<TeamManagerAssignment>();

    public DbSet<Tournament> Tournaments => Set<Tournament>();

    public DbSet<TournamentRegistration> TournamentRegistrations => Set<TournamentRegistration>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlatformDbContext).Assembly);
    }
}