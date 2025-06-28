using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Application.Commands.GoinTransactions;

public record GoinTransactionCreateCommand(
    ulong Id,
    GoinWallet Sender,
    GoinWallet Receiver,
    ulong Goin
);