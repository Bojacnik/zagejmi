using SharedKernel;

namespace Zagejmi.Domain.Community.Goin;

public sealed class GoinTransaction(
    Guid id,
    GoinWallet sender,
    GoinWallet receiver,
    Goin goin
) : Entity<Guid>(id)
{
    public GoinWallet Sender => sender;
    public GoinWallet Receiver => receiver;
    public Goin Goin => goin;
}