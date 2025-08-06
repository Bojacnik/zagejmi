using Zagejmi.Domain.Entity;

namespace Zagejmi.Domain.Community.Goin;

public sealed class GoinTransaction(
    Guid id,
    GoinWallet Sender,
    GoinWallet Receiver,
    Goin Goin
) : Entity<Guid>(id);