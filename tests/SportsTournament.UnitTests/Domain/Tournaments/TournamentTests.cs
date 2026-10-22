using SportsTournament.Domain.Enums;
using SportsTournament.Domain.Exceptions;
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
            startDate: startDate ?? new DateOnly(2027, 11, 1),
            endDate: endDate ?? new DateOnly(2027, 11, 30),
            maxTeams: maxTeams,
            playersOnFieldPerTeam: 11);
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

    [Fact]
    public void Constructor_WhenPlayersOnFieldPerTeamIsZero_ThrowsDomainValidationException()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new Tournament(
                "Test Tournament",
                Guid.NewGuid(),
                new DateTimeOffset(
                    2026, 11, 6, 18, 0, 0, TimeSpan.Zero),
                new DateOnly(2026, 11, 7),
                new DateOnly(2026, 11, 8),
                8,
                0));

        Assert.Contains("Players on field per team must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Constructor_WhenDeadlineIsOnStartDate_ThrowsArgumentException()
    {
        var startDate = new DateOnly(2026, 11, 7);
        var registrationDeadline = new DateTimeOffset(2026, 11, 7, 0, 0, 0, TimeSpan.Zero);

        Assert.Throws<DomainValidationException>(() =>
            new Tournament(
                "Test Tournament",
                Guid.NewGuid(),
                registrationDeadline,
                startDate,
                new DateOnly(2026, 11, 8),
                8,
                11));
    }

    [Fact]
    public void Constructor_WhenDeadlineIsAfterStartDate_ThrowsArgumentException()
    {
        var startDate = new DateOnly(2026, 11, 7);
        var registrationDeadline = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);

        Assert.Throws<DomainValidationException>(() =>
            new Tournament(
                "Test Tournament",
                Guid.NewGuid(),
                registrationDeadline,
                startDate,
                new DateOnly(2026, 11, 9),
                8,
                11));
    }

    [Fact]
    public void Constructor_WhenLocalDeadlineDateEqualsStartDateButUtcDateIsPreviousDay_DoesNotThrow()
    {
        var startDate = new DateOnly(2026, 11, 7);

        // 7 ноября, 02:00 UTC+3 = 6 ноября, 23:00 UTC.
        var registrationDeadline = new DateTimeOffset(2026, 11, 7, 2, 0, 0, TimeSpan.FromHours(3));

        var tournament = new Tournament(
            "Test Tournament",
            Guid.NewGuid(),
            registrationDeadline,
            startDate,
            new DateOnly(2026, 11, 8),
            8,
            11);

        Assert.Equal(startDate, tournament.StartDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ThrowsArgumentException(string name)
    {
        Assert.Throws<DomainValidationException>(() =>
            new Tournament(
                name,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddDays(10),
                new DateOnly(2026, 11, 1),
                new DateOnly(2026, 11, 30),
                8,
                11));
    }

    [Fact]
    public void Constructor_WithNameLongerThan100Characters_ThrowsException()
    {
        var name = new string('A', 101);

        Assert.Throws<DomainValidationException>(() =>  CreateTournamentWithName(name));
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
                8,
                11));
    }

    [Fact]
    public void Constructor_WithStartDateAfterEndDate_ThrowsException()
    {
        Assert.Throws<DomainValidationException>(() => CreateTournament(startDate: new DateOnly(2026, 11, 30), endDate: new DateOnly(2026, 11, 1)));
    }

    [Fact]
    public void Constructor_WithLessThanThreeTeams_ThrowsException()
    {
        Assert.Throws<DomainOperationNotAllowedException>(() => CreateTournament(maxTeams: 2));
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

        Assert.Throws<DomainOperationNotAllowedException>(() => tournament.OpenRegistration(now));

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

        Assert.Throws<DomainOperationNotAllowedException>(tournament.Start);

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

        Assert.Throws<DomainOperationNotAllowedException>(tournament.Cancel);

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
            8,
            11);
    }
}