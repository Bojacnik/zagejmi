using SharedKernel;
using Zagejmi.Domain.Events;

namespace Zagejmi.Domain.Community.Goin;

public sealed class GoinTransaction(
    ulong id,
    GoinWallet sender,
    GoinWallet receiver,
    ulong goin
) : Entity<ulong>(id)
{
    public GoinWallet Sender => sender;
    public GoinWallet Receiver => receiver;
    public ulong Goin => goin;
}