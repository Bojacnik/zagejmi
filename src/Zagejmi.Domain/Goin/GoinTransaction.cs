using System;

using Zagejmi.Domain.Abstractions;

namespace Zagejmi.Domain.Goin;

public sealed class GoinTransaction : Aggregate
{
    public GoinTransaction(Guid id, Guid sender, Guid receiver, Goin goin)
        : base(id)
    {
        this.Sender = sender;
        this.Receiver = receiver;
        this.Goin = goin;
    }

    public Guid Sender { get; private set; }

    public Guid Receiver { get; private set; }

    public Goin Goin { get; private set; }

    protected override void Apply(IDomainEvent domainEvent)
    {
    }
}