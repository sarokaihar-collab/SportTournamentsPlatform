using SportsTournament.Domain;
using SportsTournament.Domain.Enums;

namespace SportsTournament.UnitTests.Domain.Tournaments;

public sealed class TournamentRegistrationTests
{
    [Theory]
    [InlineData(RegistrationStatus.Confirmed)]
    [InlineData(RegistrationStatus.Waitlisted)]
    public void Constructor_WithValidData_CreatesRegistration(
        RegistrationStatus initialStatus)
    {
        var tournamentId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var registration = new TournamentRegistration(tournamentId, teamId, initialStatus);

        Assert.NotEqual(Guid.Empty, registration.Id);
        Assert.Equal(tournamentId, registration.TournamentId);
        Assert.Equal(teamId, registration.TeamId);
        Assert.Equal(initialStatus, registration.Status);
        Assert.NotEqual(default, registration.RegisteredAtUtc);
    }

    [Fact]
    public void Constructor_WithEmptyTournamentId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new TournamentRegistration(Guid.Empty, Guid.NewGuid(), RegistrationStatus.Confirmed));
    }

    [Fact]
    public void Constructor_WithEmptyTeamId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new TournamentRegistration(Guid.NewGuid(), Guid.Empty, RegistrationStatus.Confirmed));
    }

    [Fact]
    public void Constructor_WithCancelledStatus_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TournamentRegistration(Guid.NewGuid(), Guid.NewGuid(), RegistrationStatus.Cancelled));
    }

    [Fact]
    public void ConfirmFromWaitlist_WhenWaitlisted_ChangesStatus()
    {
        var registration = CreateRegistration(RegistrationStatus.Waitlisted);

        registration.ConfirmFromWaitlist();

        Assert.Equal(RegistrationStatus.Confirmed, registration.Status);
    }

    [Fact]
    public void ConfirmFromWaitlist_WhenAlreadyConfirmed_ThrowsException()
    {
        var registration = CreateRegistration(RegistrationStatus.Confirmed);

        Assert.Throws<InvalidOperationException>(registration.ConfirmFromWaitlist);
        Assert.Equal(RegistrationStatus.Confirmed, registration.Status);
    }

    [Theory]
    [InlineData(RegistrationStatus.Confirmed)]
    [InlineData(RegistrationStatus.Waitlisted)]
    public void Cancel_WhenNotCancelled_ChangesStatus(RegistrationStatus initialStatus)
    {
        var registration = CreateRegistration(initialStatus);

        registration.Cancel();

        Assert.Equal(RegistrationStatus.Cancelled, registration.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsException()
    {
        var registration = CreateRegistration(RegistrationStatus.Confirmed);

        registration.Cancel();

        Assert.Throws<InvalidOperationException>(registration.Cancel);
        Assert.Equal(RegistrationStatus.Cancelled, registration.Status);
    }

    [Fact]
    public void ReRegister_WhenCancelled_UpdatesStatusAndRegistrationTime()
    {
        var registration = new TournamentRegistration(Guid.NewGuid(), Guid.NewGuid(), RegistrationStatus.Confirmed);

        registration.Cancel();

        var registrationId = registration.Id;
        var newRegistrationTime = new DateTimeOffset(2026, 10, 16, 10, 0, 0, TimeSpan.FromHours(3));

        registration.ReRegister(RegistrationStatus.Waitlisted, newRegistrationTime);

        Assert.Equal(RegistrationStatus.Waitlisted, registration.Status);
        Assert.Equal(newRegistrationTime.ToUniversalTime(), registration.RegisteredAtUtc);
        Assert.Equal(registrationId, registration.Id);
    }

    [Fact]
    public void ReRegister_WhenNotCancelled_ThrowsInvalidOperationException()
    {
        var registration = new TournamentRegistration(Guid.NewGuid(), Guid.NewGuid(), RegistrationStatus.Confirmed);

        Assert.Throws<InvalidOperationException>(() => registration.ReRegister(RegistrationStatus.Waitlisted, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void ReRegister_WhenStatusIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        var registration = new TournamentRegistration(Guid.NewGuid(), Guid.NewGuid(), RegistrationStatus.Confirmed);

        registration.Cancel();

        Assert.Throws<ArgumentOutOfRangeException>(() => registration.ReRegister(RegistrationStatus.Cancelled, DateTimeOffset.UtcNow));
    }

    private static TournamentRegistration CreateRegistration(RegistrationStatus status)
    {
        return new TournamentRegistration(Guid.NewGuid(), Guid.NewGuid(), status);
    }
}