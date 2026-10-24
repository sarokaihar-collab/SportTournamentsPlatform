namespace SportsTournament.Application.Common.Outbox;

public sealed class OutboxProcessingSettings
{
    public const string SectionName = "Outbox:Processing";

    public int BatchSize { get; set; } = 10;

    public TimeSpan LockDuration { get; set; } = TimeSpan.FromMinutes(1);

    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(5);

    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
}