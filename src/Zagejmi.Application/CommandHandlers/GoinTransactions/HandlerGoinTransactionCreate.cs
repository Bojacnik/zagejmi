using LanguageExt;
using MassTransit;
using Serilog;
using SharedKernel.Failures;
using Zagejmi.Application.Commands.GoinTransactions;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Repository;

namespace Zagejmi.Application.CommandHandlers.GoinTransactions;

public class HandlerGoinTransactionCreate(
    IRepositoryGoinTransactionWrite repositoryWrite,
    IRepositoryGoinTransactionRead repositoryRead) : IConsumer<CommandGoinTransactionCreate>
{
    public async Task Consume(ConsumeContext<CommandGoinTransactionCreate> context)
    {
        CommandGoinTransactionCreate command = context.Message;

        // Validate
        var transactionsForSender = new GoinTransaction(command.Id, command.Sender, command.Receiver, command.Goin);

        var cts = new CancellationTokenSource();
        Either<Failure, List<GoinTransaction>> transactions =
            await repositoryRead.GetBySenderAsync(command.Sender, cts.Token);
        transactions
            .Right((list) =>
            {
                if (GetWalletsGoins(command.Sender, list) < command.Goin)
                    throw new ArgumentException("Not enough Goins in wallet to be able to send");

                CancellationTokenSource cancellationTokenSource = new();
                Task<Either<Failure, Unit>> result =
                    repositoryWrite.CreateAsync(transactionsForSender, cancellationTokenSource.Token);
                result.Wait(cancellationTokenSource.Token);
                if (result.IsFaulted)
                {
                    Log.Error(result.Exception, "Failed to send transaction");
                    return;
                }

                if (result.IsCompletedSuccessfully)
                {
                    Either<Failure, Unit> completed = result.Result;
                    completed
                        .Right(_ => { Log.Information("Goin transaction created"); })
                        .Left(failure => { Log.Error(failure.Message, "Goin transaction creation failed"); });
                }
            })
            .Left(f => { Log.Error(f.Message, "Goin transaction creation failed"); });
    }

    private static ulong GetWalletsGoins(GoinWallet wallet, List<GoinTransaction> transactions)
    {
        decimal sum = transactions
            .Where(e => e.Sender.Id == wallet.Id || e.Receiver.Id == wallet.Id)
            .Sum(decimal (transaction) =>
            {
                if (transaction.Sender.Id == wallet.Id)
                {
                    return -(decimal)transaction.Goin;
                }

                return transaction.Goin;
            });
        return (ulong)sum;
    }
}