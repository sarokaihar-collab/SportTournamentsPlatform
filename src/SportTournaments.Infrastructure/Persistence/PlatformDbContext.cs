using Microsoft.EntityFrameworkCore;
using SportTournaments.Application.Common.Interfaces;
using SportTournaments.Domain;
using SportTournaments.Domain.Common;
using SportTournaments.Domain.Teams;
using SportTournaments.Domain.Tournaments;
using SportTournaments.Infrastructure.Persistence.Outbox;

namespace SportTournaments.Infrastructure.Persistence;

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