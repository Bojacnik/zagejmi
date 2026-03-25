using System.ComponentModel.DataAnnotations;

namespace Zagejmi.Write.Infrastructure.EventStore;

/// <summary>
/// Represents an event stored in the application's primary event log table.
/// This is the long-term source of truth for the system.
/// </summary>
public class StoredEvent
{
    [Key]
    public Guid Id { get; init; }
    public Guid AggregateId { get; init; }
    public required string EventType { get; init; }
    public required string Data { get; init; }
    public DateTime Timestamp { get; init; }
}