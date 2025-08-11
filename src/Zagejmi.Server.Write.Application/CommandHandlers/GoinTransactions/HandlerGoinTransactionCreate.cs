using MassTransit;
using Zagejmi.Server.Write.Application.Commands.GoinTransactions;
using Zagejmi.Server.Write.Domain.Repository;

namespace Zagejmi.Server.Write.Application.CommandHandlers.GoinTransactions;

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