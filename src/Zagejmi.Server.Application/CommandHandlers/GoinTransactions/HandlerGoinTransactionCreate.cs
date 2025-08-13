using MassTransit;
using Zagejmi.Server.Application.Commands.GoinTransactions;
using Zagejmi.Server.Domain.Repository;

namespace Zagejmi.Server.Application.CommandHandlers.GoinTransactions;

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