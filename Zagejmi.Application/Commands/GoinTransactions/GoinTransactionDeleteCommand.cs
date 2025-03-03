using MediatR;
using Zagejmi.Application.Events.GoinTransactions;

namespace Zagejmi.Application.Commands.GoinTransactions;

public class GoinTransactionDeleteCommand(ulong id)
    : IRequest<GoinTransactionDeletedEvent>;