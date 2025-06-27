using SharedKernel;
using Zagejmi.Domain.Events;

namespace Zagejmi.Domain.Community.Goin;

public sealed class GoinTransaction(
    ulong id,
    GoinWallet sender,
    GoinWallet receiver,
    ulong goin
) : Entity<IGoinTransactionEvent>
{
    public override ulong Id { get; } = id;
    protected override ulong Version { get; set; }
}