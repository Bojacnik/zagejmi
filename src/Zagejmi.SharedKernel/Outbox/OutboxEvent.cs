namespace SharedKernel.Outbox;

public record OutboxEvent
{
    public required Guid Id { get; init; } = Guid.NewGuid();
    public required EventTypeDomain EventType { get; init; }
    public required string Content { get; init; }
    public required DateTime OccurredOnUtc { get; init; }
    public DateTime? ProcessedOnUtc { get; init; }
    public string? Error { get; init; }
}