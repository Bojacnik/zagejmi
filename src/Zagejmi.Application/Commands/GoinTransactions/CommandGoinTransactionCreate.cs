using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Application.Commands.GoinTransactions;

public record CommandGoinTransactionCreate(
    ulong Id,
    GoinWallet Sender,
    GoinWallet Receiver,
    ulong Goin
);