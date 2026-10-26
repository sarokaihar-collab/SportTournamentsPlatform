namespace SportTournaments.Api.Contracts.Tournaments;

using System.ComponentModel.DataAnnotations;

public sealed record CreateTournamentRequest(
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    string Name,
    [Required(ErrorMessage = "Registration deadline is required.")]
    DateTimeOffset? RegistrationDeadline,
    [Required(ErrorMessage = "Start date is required.")]
    DateOnly? StartDate,
    [Required(ErrorMessage = "End date is required.")]
    DateOnly ? EndDate,
    [Range(3, int.MaxValue, ErrorMessage = "MaxTeams must be at least 3.")]
    int MaxTeams,
    [Range(1,int.MaxValue, ErrorMessage = "PlayersOnFieldPerTeam must be greater than zero.")]
    int PlayersOnFieldPerTeam)
    : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RegistrationDeadline is { } deadline && StartDate is { } startDate)
        {
            var deadlineDateUtc = DateOnly.FromDateTime(deadline.UtcDateTime);

            if (deadlineDateUtc >= startDate)
            {
                yield return new ValidationResult("Registration deadline must be before the tournament start date in UTC.", [nameof(RegistrationDeadline)]);
            }
        }

        if (StartDate is { } start && EndDate is { } end && start > end)
        {
            yield return new ValidationResult("Tournament end date must be on or after start date.", [nameof(StartDate)]);
        }
    }
}