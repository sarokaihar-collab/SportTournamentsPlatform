namespace SportTournaments.Domain.Exceptions;

public sealed class DomainValidationException : ArgumentException
{
    public DomainValidationException(string message, string? paramName = null)
        : base(message, paramName)
    {
    }
}