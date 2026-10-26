using SportTournaments.Api.Contracts.Tournaments;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SportTournaments.UnitTests.Api.Contracts.Tournaments;

public sealed class CreateTournamentRequestTests
{
    [Fact]
    public void Validate_WhenNameIsEmpty_ReturnsError()
    {
        var request = CreateValidRequest(name: "");

        var results = Validate(request);

        Assert.Contains(results, result => result.ErrorMessage == "Name is required.");
    }

    [Fact]
    public void Validate_WhenNameExceeds100Characters_ReturnsError()
    {
        var request = CreateValidRequest(name: new string('A', 101));

        var results = Validate(request);

        Assert.Contains(results, result => result.ErrorMessage == "Name cannot exceed 100 characters.");
    }

    [Fact]
    public void Validate_WhenMaxTeamsIsLessThanThree_ReturnsError()
    {
        var request = CreateValidRequest(maxTeams: 2);

        var results = Validate(request);

        Assert.Contains(results, result => result.ErrorMessage == "MaxTeams must be at least 3.");
    }

    [Fact]
    public void Validate_WhenDeadlineIsOnStartDateInUtc_ReturnsError()
    {
        var request = CreateValidRequest(deadline: new DateTimeOffset(2026, 11, 7, 12, 0, 0, TimeSpan.FromHours(3)));

        var results = Validate(request);

        Assert.Contains(results, result => result.ErrorMessage == "Registration deadline must be before the tournament start date in UTC.");
    }

    [Fact]
    public void Validate_WhenUtcDeadlineIsPreviousDay_ReturnsNoDeadlineError()
    {
        var request = CreateValidRequest(deadline: new DateTimeOffset(2026, 11, 7, 2, 0, 0, TimeSpan.FromHours(3)));

        var results = Validate(request);

        Assert.DoesNotContain(results, result => result.ErrorMessage == "Registration deadline must be before the tournament start date in UTC.");
    }

    [Fact]
    public void Validate_WhenEndDateIsBeforeStartDate_ReturnsError()
    {
        var request = CreateValidRequest(startDate: new DateOnly(2026, 11, 8), endDate: new DateOnly(2026, 11, 7));

        var results = Validate(request);

        Assert.Contains(results, result => result.ErrorMessage == "Tournament end date must be on or after start date.");
    }

    private static CreateTournamentRequest CreateValidRequest(
        string name = "Test Tournament",
        DateTimeOffset? deadline = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        int maxTeams = 8,
        int playersOnFieldPerTeam = 3)
    {
        return new CreateTournamentRequest(
            name,
            deadline ?? new DateTimeOffset(
                2026, 11, 6, 18, 0, 0, TimeSpan.Zero),
            startDate ?? new DateOnly(2026, 11, 7),
            endDate ?? new DateOnly(2026, 11, 8),
            maxTeams,
            playersOnFieldPerTeam);
    }

    [Fact]
    public void Validate_WhenRegistrationDeadlineIsMissing_ReturnsError()
    {
        var request = new CreateTournamentRequest(
            "Test Tournament",
            null,
            new DateOnly(2026, 11, 7),
            new DateOnly(2026, 11, 8),
            8,
            11);

        var results = Validate(request);

        Assert.Contains(results, result => result.ErrorMessage == "Registration deadline is required.");
    }

    [Fact]
    public void Validate_WhenStartDateIsMissing_ReturnsError()
    {
        var request = new CreateTournamentRequest(
            "Test Tournament",
            new DateTimeOffset(2026, 11, 6, 18, 0, 0, TimeSpan.Zero),
            null,
            new DateOnly(2026, 11, 8),
            8,
            11);

        var results = Validate(request);

        Assert.Contains(results, result => result.ErrorMessage == "Start date is required.");
    }

    [Fact]
    public void Validate_WhenEndDateIsMissing_ReturnsError()
    {
        var request = new CreateTournamentRequest(
            "Test Tournament",
            new DateTimeOffset(2026, 11, 6, 18, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 11, 7),
            null,
            8,
            11);

        var results = Validate(request);

        Assert.Contains(results, result => result.ErrorMessage == "End date is required.");
    }



    private static List<ValidationResult> Validate(CreateTournamentRequest request)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            results,
            validateAllProperties: true);

        var requestType = typeof(CreateTournamentRequest);
        var constructor = requestType.GetConstructors().Single();

        foreach (var parameter in constructor.GetParameters())
        {
            var attributes = parameter.GetCustomAttributes<ValidationAttribute>(inherit: true).ToArray();

            if (attributes.Length == 0)
            {
                continue;
            }

            var property = requestType.GetProperty(
                parameter.Name!,
                BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.IgnoreCase)
                ?? throw new InvalidOperationException(
                    $"Property for parameter '{parameter.Name}' was not found.");

            Validator.TryValidateValue(
                property.GetValue(request),
                new ValidationContext(request)
                {
                    MemberName = property.Name
                },
                results,
                attributes);
        }

        return results;
    }
}