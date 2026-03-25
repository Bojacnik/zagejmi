using Zagejmi.Contracts.Abstractions;

namespace Zagejmi.Contracts.Messages;

/// <summary>
///     Represents a message envelope that encapsulates a message payload along with its associated metadata (headers).
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public sealed record Envelope<TMessage>
    where TMessage : IMessage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Envelope"/> class.
    /// </summary>
    /// <param name="payload">Payload of the message to be encapsulated within the envelope.</param>
    /// <param name="metadata">
    ///     Optional metadata for the message. If not provided, a new instance of
    ///     <see cref="MessageHeaders" /> will be created with default values.
    /// </param>
    public Envelope(TMessage payload, MessageHeaders? metadata = null)
    {
        this.Payload = payload;
        this.Metadata = metadata ?? new MessageHeaders();
    }

    /// <summary>
    ///     Gets the message payload, which is the actual content of the message being transmitted.
    /// </summary>
    public TMessage Payload { get; init; }

    /// <summary>
    ///     Gets the message headers, which contain metadata about the message such as message ID, correlation ID, causation
    ///     ID, creation timestamp, and source information.
    /// </summary>
    public MessageHeaders Metadata { get; init; }
}