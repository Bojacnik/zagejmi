using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Application.Commands.GoinTransactions;

public record CommandGoinTransactionCreate(
    ulong SenderGoinWalletId,
    ulong ReceiverGoinWalletId,
    ulong GoinAmount
);