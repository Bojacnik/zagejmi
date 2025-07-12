using SharedKernel;

namespace Zagejmi.Domain.Community.Goin;

public sealed record GoinTransaction(
    Guid Id,
    GoinWallet Sender,
    GoinWallet Receiver,
    Goin Goin
) : Entity<Guid>(Id);