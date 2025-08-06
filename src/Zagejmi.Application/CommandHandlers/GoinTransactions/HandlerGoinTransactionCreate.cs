using LanguageExt;
using MassTransit;
using Serilog;
using Zagejmi.Application.Commands.GoinTransactions;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Repository;
using Zagejmi.SharedKernel.Failures;

namespace Zagejmi.Application.CommandHandlers.GoinTransactions;

public class HandlerGoinTransactionCreate(
    IRepositoryGoinTransactionWrite repositoryTransactionWrite,
    IRepositoryGoinTransactionRead repositoryTransactionRead,
    IRepositoryGoinWalletRead repositoryGoinRead) : IConsumer<CommandGoinTransactionCreate>
{
    public Task Consume(ConsumeContext<CommandGoinTransactionCreate> context)
    {
        throw new NotImplementedException();
    }
}