using System;

using Zagejmi.Contracts.Abstractions;

namespace Zagejmi.Contracts.Messages.Factory;

public static class Envelope
{
    public static Envelope<T> Create<T>(
        T message,
        Guid? correlationId = null,
        Guid? causationId = null,
        string? source = null)
        where T : IMessage
    {
        return new Envelope<T>(
            message,
            new MessageHeaders
            {
                CorrelationId = correlationId,
                CausationId = causationId,
                Source = source
            });
    }
}