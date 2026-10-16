using SportsTournament.Domain.Teams;

namespace SportsTournament.UnitTests.Domain.Teams;

public sealed class TeamManagerAssignmentTests
{
    [Fact]
    public void Constructor_WithValidIds_CreatesAssignment()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        // Act
        var assignment = new TeamManagerAssignment(teamId, managerId);

        // Assert
        Assert.NotEqual(Guid.Empty, assignment.Id);
        Assert.Equal(teamId, assignment.TeamId);
        Assert.Equal(managerId, assignment.ManagerId);
        Assert.NotEqual(default, assignment.AssignedAtUtc);
    }

    [Fact]
    public void Constructor_WithEmptyTeamId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>  new TeamManagerAssignment(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_WithEmptyManagerId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>  new TeamManagerAssignment(Guid.NewGuid(), Guid.Empty));
    }
}