using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Zagejmi.Server.Infrastructure;

/// <summary>
/// Represents an event stored in the application's primary event log table.
/// This is the long-term source of truth for the system.
/// </summary>
[Index(nameof(AggregateId), Name = "IX_AggregateId")]
public class StoredEvent
{
    [Key]
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
