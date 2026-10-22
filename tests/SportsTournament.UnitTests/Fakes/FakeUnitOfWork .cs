using SportsTournament.Application.Common;

namespace SportsTournament.UnitTests.Common.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Exception? SaveChangesException { get; init; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SaveChangesCallCount++;

        return SaveChangesException is null ? Task.FromResult(1) : Task.FromException<int>(SaveChangesException);
    }
}