using MassTransit;
using Zagejmi.Write.Domain.Repository;
using Zagejmi.Write.Application.Commands.GoinTransactions;

namespace Zagejmi.Write.Application.CommandHandlers.GoinTransactions;

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