using Zagejmi.Server.Write.Domain.Entity;

namespace Zagejmi.Server.Write.Domain.Community.Goin;

public sealed class GoinTransaction(
    Guid id,
    GoinWallet Sender,
    GoinWallet Receiver,
    Goin Goin
) : Entity<Guid>(id);