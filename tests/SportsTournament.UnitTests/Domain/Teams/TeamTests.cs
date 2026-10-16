using SportsTournament.Domain.Teams;

namespace SportsTournament.UnitTests.Domain.Teams;

public sealed class TeamTests
{
    [Fact]
    public void Constructor_WithValidName_CreatesTeamAndTrimsName()
    {
        // Arrange
        const string name = "  FC Barcelona  ";

        // Act
        var team = new Team(name);

        // Assert
        Assert.NotEqual(Guid.Empty, team.Id);
        Assert.Equal("FC Barcelona", team.Name);
        Assert.NotEqual(default, team.CreatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ThrowsArgumentException(string name)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Team(name));
    }

    [Fact]
    public void Constructor_WithNameLongerThan100Characters_ThrowsException()
    {
        // Arrange
        var name = new string('A', 101);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Team(name));
    }

    [Fact]
    public void Rename_WithValidName_UpdatesNameAndTrimsWhitespace()
    {
        // Arrange
        var team = new Team("FC Barcelona");

        // Act
        team.Rename("  Real Madrid  ");

        // Assert
        Assert.Equal("Real Madrid", team.Name);
    }

    [Fact]
    public void Rename_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var team = new Team("FC Barcelona");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => team.Rename(" "));
    }
}