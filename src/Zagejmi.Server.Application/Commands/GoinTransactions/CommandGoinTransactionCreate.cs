namespace Zagejmi.Server.Application.Commands.GoinTransactions;

public record CommandGoinTransactionCreate(
    Guid SenderGoinWalletId,
    Guid ReceiverGoinWalletId,
    ulong GoinAmount
);