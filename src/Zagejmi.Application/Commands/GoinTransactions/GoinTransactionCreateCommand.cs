using MediatR;
using Zagejmi.Domain.Community.Goin;
using Zagejmi.Domain.Events.GoinTransactions;

namespace Zagejmi.Application.Commands.GoinTransactions;

public class GoinTransactionCreateCommand(
    ulong id,
    GoinWallet sender,
    GoinWallet receiver,
    ulong goin
) : IRequest<GoinTransactionCreatedEvent>
{
}