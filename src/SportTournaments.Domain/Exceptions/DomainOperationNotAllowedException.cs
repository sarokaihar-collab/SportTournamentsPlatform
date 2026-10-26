namespace SportTournaments.Domain.Exceptions;

public sealed class DomainOperationNotAllowedException
    : InvalidOperationException
{
    public DomainOperationNotAllowedException(string message)
        : base(message)
    {
    }
}