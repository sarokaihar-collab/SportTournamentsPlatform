using SportsTournament.Domain.Enums;
using SportsTournament.Domain.Tournaments;

namespace SportsTournament.UnitTests.Domain.Tournaments;

public sealed class TournamentTests
{
    private static Tournament CreateTournament(DateTimeOffset? deadline = null, DateOnly? startDate = null, DateOnly? endDate = null, int maxTeams = 8)
    {
        return new Tournament(
            name: "  Summer Cup  ",
            organizerId: Guid.NewGuid(),
            registrationDeadlineUtc: deadline ?? DateTimeOffset.UtcNow.AddDays(10),
            startDate: startDate ?? new DateOnly(2026, 11, 1),
            endDate: endDate ?? new DateOnly(2026, 11, 30),
            maxTeams: maxTeams);
    }

    [Fact]
    public void Constructor_WithValidData_CreatesDraftTournament()
    {
        var tournament = CreateTournament();

        Assert.NotEqual(Guid.Empty, tournament.Id);
        Assert.Equal("Summer Cup", tournament.Name);
        Assert.Equal(TournamentStatus.Draft, tournament.Status);
        Assert.Equal(8, tournament.MaxTeams);
        Assert.NotEqual(default, tournament.CreatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ThrowsArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Tournament(
                name,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddDays(10),
                new DateOnly(2026, 11, 1),
                new DateOnly(2026, 11, 30),
                8));
    }

    [Fact]
    public void Constructor_WithNameLongerThan100Characters_ThrowsException()
    {
        var name = new string('A', 101);

        Assert.Throws<ArgumentOutOfRangeException>(() =>  CreateTournamentWithName(name));
    }

    [Fact]
    public void Constructor_WithEmptyOrganizerId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Tournament(
                "Summer Cup",
                Guid.Empty,
                DateTimeOffset.UtcNow.AddDays(10),
                new DateOnly(2026, 11, 1),
                new DateOnly(2026, 11, 30),
                8));
    }

    [Fact]
    public void Constructor_WithStartDateAfterEndDate_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => CreateTournament(startDate: new DateOnly(2026, 11, 30), endDate: new DateOnly(2026, 11, 1)));
    }

    [Fact]
    public void Constructor_WithLessThanThreeTeams_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateTournament(maxTeams: 2));
    }

    [Fact]
    public void OpenRegistration_WithFutureDeadline_ChangesStatus()
    {
        var now = DateTimeOffset.UtcNow;
        var tournament = CreateTournament(deadline: now.AddDays(1));

        tournament.OpenRegistration(now);

        Assert.Equal(TournamentStatus.RegistrationOpen, tournament.Status);
    }

    [Fact]
    public void OpenRegistration_WithExpiredDeadline_ThrowsException()
    {
        var now = DateTimeOffset.UtcNow;
        var tournament = CreateTournament(deadline: now);

        Assert.Throws<InvalidOperationException>(() => tournament.OpenRegistration(now));

        Assert.Equal(TournamentStatus.Draft, tournament.Status);
    }

    [Fact]
    public void CloseRegistration_WhenRegistrationIsOpen_ChangesStatus()
    {
        var tournament = CreateTournament();
        tournament.OpenRegistration(DateTimeOffset.UtcNow);

        tournament.CloseRegistration();

        Assert.Equal(TournamentStatus.RegistrationClosed, tournament.Status);
    }

    [Fact]
    public void Start_WhenRegistrationIsNotClosed_ThrowsException()
    {
        var tournament = CreateTournament();

        Assert.Throws<InvalidOperationException>(tournament.Start);

        Assert.Equal(TournamentStatus.Draft, tournament.Status);
    }

    [Fact]
    public void Complete_WhenTournamentIsInProgress_ChangesStatus()
    {
        var tournament = CreateTournament();
        tournament.OpenRegistration(DateTimeOffset.UtcNow);
        tournament.CloseRegistration();
        tournament.Start();

        tournament.Complete();

        Assert.Equal(TournamentStatus.Completed, tournament.Status);
    }

    [Fact]
    public void Cancel_WhenTournamentIsInProgress_ThrowsException()
    {
        var tournament = CreateTournament();
        tournament.OpenRegistration(DateTimeOffset.UtcNow);
        tournament.CloseRegistration();
        tournament.Start();

        Assert.Throws<InvalidOperationException>(tournament.Cancel);

        Assert.Equal(TournamentStatus.InProgress, tournament.Status);
    }

    [Fact]
    public void ChangeMaxTeams_WhenRegistrationIsOpen_UpdatesCapacity()
    {
        var tournament = CreateTournament();
        tournament.OpenRegistration(DateTimeOffset.UtcNow);

        tournament.ChangeMaxTeams(newMaxTeams: 12, confirmedTeamsCount: 5);

        Assert.Equal(12, tournament.MaxTeams);
    }

    [Fact]
    public void ChangeMaxTeams_BelowConfirmedCount_ThrowsException()
    {
        var tournament = CreateTournament();
        tournament.OpenRegistration(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => tournament.ChangeMaxTeams(newMaxTeams: 4, confirmedTeamsCount: 5));

        Assert.Equal(8, tournament.MaxTeams);
    }
    private static Tournament CreateTournamentWithName(string name)
    {
        return new Tournament(
            name,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(10),
            new DateOnly(2026, 11, 1),
            new DateOnly(2026, 11, 30),
            8);
    }
}