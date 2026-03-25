using System;

namespace Zagejmi.Contracts.Messages;

/// <summary>
///     Represents the headers of a message, containing metadata such as message ID, correlation ID, causation ID, creation
///     timestamp, and source information.
/// </summary>
public sealed record MessageHeaders
{
    /// <summary>
    ///     Gets the unique identifier for the message.
    ///     This ID is generated when the message is created and is used to track the message throughout its lifecycle.
    /// </summary>
    public Guid MessageId { get; init; } = Guid.NewGuid();

    /// <summary>
    ///     Gets the correlation ID for the message, which is used to group related messages together.
    /// </summary>
    public Guid? CorrelationId { get; init; }

    /// <summary>
    ///     Gets the causation ID for the message, which indicates the message that caused this message to be created.
    /// </summary>
    public Guid? CausationId { get; init; }

    /// <summary>
    ///     Gets the timestamp indicating when the message was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets the source of the message, which can be used to identify where the message originated from (e.g., a specific
    ///     service or component).
    /// </summary>
    public string? Source { get; init; }
}