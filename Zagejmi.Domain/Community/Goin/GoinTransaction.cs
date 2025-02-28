using Zagejmi.Domain.Events;

namespace Zagejmi.Domain.Community.Goin;

public class GoinTransaction(
    ulong id,
    GoinWallet sender,
    GoinWallet receiver,
    ulong goin
) : Entity<ITransactionEvent>
{
    public override ulong Id { get; } = id;
    protected override ulong Version { get; set; }
}