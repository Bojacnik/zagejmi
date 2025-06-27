using MediatR;
using Zagejmi.Domain.Events.GoinTransactions;

namespace Zagejmi.Application.Commands.GoinTransactions;

public class GoinTransactionDeleteCommand(ulong id)
    : IRequest<GoinTransactionDeletedEvent>;