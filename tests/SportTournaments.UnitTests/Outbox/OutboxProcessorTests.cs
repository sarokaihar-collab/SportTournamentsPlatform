using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SportTournaments.Application.Common.Outbox;

namespace SportTournaments.UnitTests.Common.Outbox;

public sealed class OutboxProcessorTests
{
    private static readonly string[] TestMessageTypes =
    [
        "test-message.v1"
    ];

    [Fact]
    public async Task ProcessBatchAsync_WhenDispatchSucceeds_MarksMessageProcessed()
    {
        // Arrange
        var message = CreateMessage();

        var store = new FakeOutboxMessageStore
        {
            Messages = [message]
        };

        var dispatcher = new FakeOutboxMessageDispatcher();
        var sut = CreateSut(store, dispatcher);

        // Act
        var processedCount = await sut.ProcessBatchAsync(TestMessageTypes);

        // Assert
        Assert.Equal(1, processedCount);

        var dispatchCall = Assert.Single(dispatcher.Calls);
        Assert.Equal(message.MessageType, dispatchCall.MessageType);
        Assert.Equal(message.Payload, dispatchCall.Payload);

        var markProcessedCall = Assert.Single(store.MarkProcessedCalls);
        Assert.Equal(message.Id, markProcessedCall.MessageId);
        Assert.Equal(message.LockToken, markProcessedCall.LockToken);

        Assert.Empty(store.RetryCalls);

        Assert.Contains(TestMessageTypes.First(), store.ClaimedMessageTypes);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenDispatchFails_SchedulesRetry()
    {
        // Arrange
        var message = CreateMessage(attemptCount: 3);

        var store = new FakeOutboxMessageStore
        {
            Messages = [message]
        };

        var dispatcher = new FakeOutboxMessageDispatcher
        {
            DispatchException = new InvalidOperationException("Simulated dispatch failure.")
        };

        var settings = CreateSettings();
        var sut = CreateSut(store, dispatcher, settings);

        // Act
        var processedCount = await sut.ProcessBatchAsync(TestMessageTypes);

        // Assert
        Assert.Equal(1, processedCount);
        Assert.Single(dispatcher.Calls);
        Assert.Empty(store.MarkProcessedCalls);

        var retryCall = Assert.Single(store.RetryCalls);

        Assert.Equal(message.Id, retryCall.MessageId);
        Assert.Equal(message.LockToken, retryCall.LockToken);

        // attemptCount = 3 => initial delay * 2^(3 - 1)
        Assert.Equal(TimeSpan.FromSeconds(20), retryCall.RetryDelay);

        // Persist the exception type, not its arbitrary message.
        Assert.Equal(nameof(InvalidOperationException), retryCall.LastError);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenLeaseLostWhileMarkingProcessed_DoesNotScheduleRetry()
    {
        // Arrange
        var message = CreateMessage();

        var store = new FakeOutboxMessageStore
        {
            Messages = [message],
            MarkProcessedException = new OutboxLeaseLostException(message.Id)
        };

        var dispatcher = new FakeOutboxMessageDispatcher();
        var sut = CreateSut(store, dispatcher);

        // Act
        var processedCount = await sut.ProcessBatchAsync(TestMessageTypes);

        // Assert
        Assert.Equal(1, processedCount);
        Assert.Single(dispatcher.Calls);
        Assert.Single(store.MarkProcessedCalls);
        Assert.Empty(store.RetryCalls);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenLeaseLostWhileSchedulingRetry_DoesNotPropagateException()
    {
        // Arrange
        var message = CreateMessage();

        var store = new FakeOutboxMessageStore
        {
            Messages = [message],
            ScheduleRetryException = new OutboxLeaseLostException(message.Id)
        };

        var dispatcher = new FakeOutboxMessageDispatcher
        {
            DispatchException = new InvalidOperationException("Simulated dispatch failure.")
        };

        var sut = CreateSut(store, dispatcher);

        // Act
        var processedCount = await sut.ProcessBatchAsync(TestMessageTypes);

        // Assert
        Assert.Equal(1, processedCount);
        Assert.Single(dispatcher.Calls);
        Assert.Empty(store.MarkProcessedCalls);

        // Scheduling was attempted, but the lease had already been lost.
        Assert.Single(store.RetryCalls);
    }

    private static OutboxProcessor CreateSut(FakeOutboxMessageStore store, FakeOutboxMessageDispatcher dispatcher, OutboxProcessingSettings? settings = null)
    {
        return new OutboxProcessor(store, dispatcher, Options.Create(settings ?? CreateSettings()), NullLogger<OutboxProcessor>.Instance);
    }

    private static OutboxProcessingSettings CreateSettings()
    {
        return new OutboxProcessingSettings
        {
            BatchSize = 10,
            LockDuration = TimeSpan.FromMinutes(1),
            InitialRetryDelay = TimeSpan.FromSeconds(5),
            MaxRetryDelay = TimeSpan.FromMinutes(1),
            PollingInterval = TimeSpan.FromSeconds(5)
        };
    }

    private static ClaimedOutboxMessage CreateMessage(int attemptCount = 1)
    {
        return new ClaimedOutboxMessage(Guid.NewGuid(), OutboxMessageTypes.TournamentListCacheInvalidationRequested, "{}", attemptCount, Guid.NewGuid());
    }

    private sealed class FakeOutboxMessageStore : IOutboxMessageStore
    {
        public IReadOnlyList<ClaimedOutboxMessage> Messages { get; init; } = Array.Empty<ClaimedOutboxMessage>();

        public IReadOnlyCollection<string> ClaimedMessageTypes { get; private set; } = Array.Empty<string>();

        public int ClaimedBatchSize { get; private set; }

        public TimeSpan ClaimedLockDuration { get; private set; }

        public List<(Guid MessageId, Guid LockToken)> MarkProcessedCalls { get; } = [];

        public List<RetryScheduleCall> RetryCalls { get; } = [];

        public Exception? MarkProcessedException { get; init; }

        public Exception? ScheduleRetryException { get; init; }

        public Task<IReadOnlyList<ClaimedOutboxMessage>> ClaimBatchAsync(IReadOnlyCollection<string> messageTypes,
            int batchSize, TimeSpan lockDuration, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ClaimedMessageTypes = messageTypes.ToArray();
            ClaimedBatchSize = batchSize;
            ClaimedLockDuration = lockDuration;

            return Task.FromResult(Messages);
        }

        public Task MarkProcessedAsync(Guid messageId, Guid lockToken, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            MarkProcessedCalls.Add((messageId, lockToken));

            if (MarkProcessedException is { } exception)
            {
                return Task.FromException(exception);
            }

            return Task.CompletedTask;
        }

        public Task ScheduleRetryAsync(Guid messageId, Guid lockToken, TimeSpan retryDelay, string? lastError,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RetryCalls.Add(new RetryScheduleCall(messageId, lockToken, retryDelay, lastError));

            if (ScheduleRetryException is { } exception)
            {
                return Task.FromException(exception);
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeOutboxMessageDispatcher : IOutboxMessageDispatcher
    {
        public List<DispatchCall> Calls { get; } = [];

        public Exception? DispatchException { get; init; }

        public Task DispatchAsync(string messageType, string payload, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Calls.Add(new DispatchCall(messageType, payload));

            if (DispatchException is { } exception)
            {
                return Task.FromException(exception);
            }

            return Task.CompletedTask;
        }
    }

    private sealed record DispatchCall(string MessageType, string Payload);

    private sealed record RetryScheduleCall(Guid MessageId, Guid LockToken, TimeSpan RetryDelay, string? LastError);
}