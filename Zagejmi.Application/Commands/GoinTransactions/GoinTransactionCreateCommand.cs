using MediatR;
using Zagejmi.Application.Events.GoinTransactions;
using Zagejmi.Domain.Community.Goin;

namespace Zagejmi.Application.Commands.GoinTransactions;

public class GoinTransactionCreateCommand(
    ulong id,
    GoinWallet sender,
    GoinWallet receiver,
    ulong goin
) : IRequest<GoinTransactionCreatedEvent>
{
}