namespace Zagejmi.Server.Write.Application.Commands.GoinTransactions;

public record CommandGoinTransactionCreate(
    Guid SenderGoinWalletId,
    Guid ReceiverGoinWalletId,
    ulong GoinAmount
);