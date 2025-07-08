using LanguageExt;
using MassTransit;
using Serilog;
using SharedKernel.Failures;
using Zagejmi.Application.Commands.GoinTransactions;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Repository;

namespace Zagejmi.Application.CommandHandlers.GoinTransactions;

public class HandlerGoinTransactionCreate(
    IRepositoryGoinTransactionWrite repositoryTransactionWrite,
    IRepositoryGoinTransactionRead repositoryTransactionRead,
    IRepositoryGoinWalletRead repositoryGoinRead) : IConsumer<CommandGoinTransactionCreate>
{
    public async Task Consume(ConsumeContext<CommandGoinTransactionCreate> context)
    {
        CommandGoinTransactionCreate command = context.Message;

        var ctss = new CancellationTokenSource();

        GoinWallet? sender = null;
        GoinWallet? receiver = null;
        try
        {
            Either<Failure, GoinWallet?> senderGoinWallet =
                await repositoryGoinRead.GetByIdAsync(context.Message.SenderGoinWalletId, ctss.Token);

            senderGoinWallet.Match(senderWallet =>
                {
                    Log.Information("Goin wallet {walletId} found", senderWallet?.Id.ToString());
                    sender = senderWallet;
                },
                failure =>
                {
                    Log.Error("Goin wallet {walletId} not found, cancelling transaction creation! {failure}",
                        context.Message.SenderGoinWalletId, failure.Message
                    );
                }
            );

            Either<Failure, GoinWallet?> receiverGoinWallet =
                await repositoryGoinRead.GetByIdAsync(context.Message.ReceiverGoinWalletId, ctss.Token);

            receiverGoinWallet.Match(receiverWallet =>
                {
                    Log.Information("Goin wallet {walletId} found", receiverWallet?.Id.ToString());
                    receiver = receiverWallet;
                },
                failure => Log.Information("Receiver wallet not found {walletId}",
                    context.Message.ReceiverGoinWalletId));
        }
        catch (ArgumentException e)
        {
            Log.Error("Goin wallet not found {e}", e.Message);
            throw;
        }

        if (receiver == null || sender == null)
        {
            throw new Exception("Goin wallet not found");
        }

        // Validate
        var transactionsForSender =
            new GoinTransaction(Guid.NewGuid(), sender, receiver, new Goin(command.GoinAmount));

        var cts = new CancellationTokenSource();
        Either<Failure, List<GoinTransaction>> outgoingTransactions =
            await repositoryTransactionRead.GetBySenderIdAsync(command.SenderGoinWalletId, cts.Token);
        Either<Failure, List<GoinTransaction>> incomingTransactions =
            await repositoryTransactionRead.GetByReceiver(command.SenderGoinWalletId, cts.Token);

        List<GoinTransaction> transactions = [];

        outgoingTransactions.Match(
            e => { transactions.AddRange(e); },
            f => { Log.Error("Error {}", f.Message); });
        incomingTransactions.Match
        (list => { transactions.AddRange(list); },
            e => { Log.Error("Error {}", e.Message); }
        );

        if (GetWalletsGoins(sender, sender.Transactions) < command.GoinAmount)
            throw new ArgumentException("Not enough Goins in wallet to be able to send");

        CancellationTokenSource cancellationTokenSource = new();
        Task<Either<Failure, Unit>> result =
            repositoryTransactionWrite.CreateAsync(transactionsForSender, cancellationTokenSource.Token);
        await result.WaitAsync(cancellationTokenSource.Token);
        if (result.IsFaulted)
        {
            Log.Error("Failed to send transaction {}", result.Exception);
            throw new Exception("Failed to send transactoin");
        }

        if (result.IsCompletedSuccessfully)
        {
            Either<Failure, Unit> completed = result.Result;
            completed
                .Right(_ => { Log.Information("Goin transaction created"); })
                .Left(failure => { Log.Error("Goin transaction creation failed {}", failure.Message); });
        }
    }

    private static ulong GetWalletsGoins(GoinWallet wallet, List<GoinTransaction> transactions)
    {
        decimal sum = transactions
            .Where(e => e.Sender.Id == wallet.Id || e.Receiver.Id == wallet.Id)
            .Sum(decimal (transaction) =>
            {
                if (transaction.Sender.Id == wallet.Id)
                {
                    return -(decimal)transaction.Goin.Amount;
                }

                return transaction.Goin.Amount;
            });
        return (ulong)sum;
    }
}