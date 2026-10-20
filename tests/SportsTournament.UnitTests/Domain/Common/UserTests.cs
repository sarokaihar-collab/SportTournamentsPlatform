using SportsTournament.Domain.Common;
using SportsTournament.Domain.Exceptions;

namespace SportsTournament.UnitTests.Domain.Common;

public sealed class UserTests
{
    [Fact]
    public void Constructor_WithValidDisplayName_CreatesUserAndTrimsName()
    {
        var user = new User("  Alex Smith  ");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Alex Smith", user.DisplayName);
        Assert.NotEqual(default, user.CreatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyDisplayName_ThrowsArgumentException(
        string displayName)
    {
        Assert.Throws<DomainValidationException>(() => new User(displayName));
    }

    [Fact]
    public void Constructor_WithDisplayNameLongerThan100Characters_ThrowsException()
    {
        var displayName = new string('A', 101);

        Assert.Throws<DomainValidationException>(() => new User(displayName));
    }

    [Fact]
    public void Rename_WithValidDisplayName_UpdatesAndTrimsName()
    {
        var user = new User("Alex Smith");

        user.Rename("  John Smith  ");

        Assert.Equal("John Smith", user.DisplayName);
    }

    [Fact]
    public void Rename_WithEmptyDisplayName_ThrowsArgumentException()
    {
        var user = new User("Alex Smith");

        Assert.Throws<DomainValidationException>(() => user.Rename(" "));
    }
}