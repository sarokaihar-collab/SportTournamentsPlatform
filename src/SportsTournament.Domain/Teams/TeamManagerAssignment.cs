namespace SportsTournament.Domain.Teams;

public sealed class TeamManagerAssignment
{
    // Constructor used by EF Core.
    private TeamManagerAssignment()
    {
    }

    public TeamManagerAssignment(Guid teamId, Guid managerId)
    {
        if (teamId == Guid.Empty)
        {
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));
        }

        if (managerId == Guid.Empty)
        {
            throw new ArgumentException("Manager ID cannot be empty.", nameof(managerId));
        }

        Id = Guid.NewGuid();
        TeamId = teamId;
        ManagerId = managerId;
        AssignedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TeamId { get; private set; }

    public Guid ManagerId { get; private set; }

    public DateTimeOffset AssignedAtUtc { get; private set; }
}